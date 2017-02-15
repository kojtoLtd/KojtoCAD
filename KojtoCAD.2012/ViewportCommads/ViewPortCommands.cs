using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using System;
using System.Runtime.InteropServices;
using KojtoCAD.Mathematics.Geometry;

#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif

[assembly: CommandClass(typeof(KojtoCAD.ViewportCommads.ViewPortCommands))]

namespace KojtoCAD.ViewportCommads
{
    using Exception = System.Exception;

    public class ViewPortCommands
    {
        private readonly Document _doc = Application.DocumentManager.MdiActiveDocument;

        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        private static ILogger _logger = NullLogger.Instance;

        public static ILogger Logger
        {
            get
            {
                return _logger;
            }
            set
            {
                _logger = value;
            }
        }

        [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PEBVAcDbViewport@@@Z")]
        private static extern int SetCurrentVPort2010(IntPtr acDbVport);
        [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PEBVAcDbViewport@@@Z")]
        private static extern int SetCurrentVPort2013(IntPtr acDbVport);

        [DllImport("Brx16.dll", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PEBVAcDbViewport@@@Z")]
        private static extern int SetCurrentVPortBCad(IntPtr acDbVport);

        /// <summary>
        /// Align view ports horizontaly
        /// </summary>
        [CommandMethod("avh")]
        public void AlignViewportsHorizontalyStart()
        {
            
            _doc.SendStringToExecute("Mvsetup ", true, false, true);
            _doc.SendStringToExecute("A ", true, false, true);
            _doc.SendStringToExecute("H ", true, false, true);
            _doc.SendStringToExecute("nea ", true, false, true);
            _doc.SendStringToExecute("nea ", true, false, true);
            _doc.SendStringToExecute("pspace ", true, false, true);
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Align view ports vertically
        /// </summary>
        [CommandMethod("avv")]
        public void AlignViewportsVerticalyStart()
        {
            
            _doc.SendStringToExecute("Mvsetup ", true, false, true);
            _doc.SendStringToExecute("A ", true, false, true);
            _doc.SendStringToExecute("V ", true, false, true);
            _doc.SendStringToExecute("nea", true, false, true);
            _doc.SendStringToExecute("nea", true, false, true);
            _doc.SendStringToExecute("pspace ", true, false, true);
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Lock a Viewport on a Layout
        /// </summary>
        [CommandMethod("vlock")]
        public void LockViewPortStart()
        {
            
            CommandLineHelper.Command("-vports", "Lock", "ON");
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Unlock a Viewport on a Layout
        /// </summary>
        [CommandMethod("vunlock")]
        public void UnlockViewPortStart()
        {
            
            CommandLineHelper.Command("-vports", "Lock", "OFF");
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Creates a viewport out of selected rectangle in model 
        /// </summary>
        [CommandMethod("cv")]
        public void CreateViewportFromModelStart()
        {
            
            using (Transaction transaction = _doc.Database.TransactionManager.StartTransaction())
            {
                #region Switch to model space

                Application.SetSystemVariable("TILEMODE", 1);

                #endregion

                #region Get View Rectangle Pts from model space

                PromptPointOptions VportRectangleOpts =
                    new PromptPointOptions("\nSpecify rectangle describing the desired view : \n");
                PromptPointResult VportRectangleRes = _doc.Editor.GetPoint(VportRectangleOpts);
                if (VportRectangleRes.Status != PromptStatus.OK)
                {
                    return;
                }

                Point3d CornerPt = VportRectangleRes.Value;
                RectangleJig Recjig = new RectangleJig(CornerPt);
                if (_doc.Editor.Drag(Recjig).Status == PromptStatus.Cancel)
                {
                    return;
                }

                Polyline ViewPline = Recjig.Polyline;
                Point3dCollection ViewPlinePts = GeometryUtility.GetPointsFromPolyline(ViewPline);
                if (ViewPlinePts.Count == 5)
                {
                    ViewPlinePts.RemoveAt(4);
                }

                #endregion

                #region Calculate View height

                double viewHeight = Math.Abs(ViewPlinePts[0].Y - ViewPlinePts[2].Y);

                #endregion

                #region Calculate View width

                double viewWidth = Math.Abs(ViewPlinePts[0].X - ViewPlinePts[2].X);

                #endregion

                #region Calculate View Center Pt

                double baseWidth = ViewPlinePts[0].X;
                if (ViewPlinePts[0].X > ViewPlinePts[2].X)
                {
                    baseWidth = ViewPlinePts[2].X;
                }

                double baseHeight = ViewPlinePts[0].Y;
                if (ViewPlinePts[0].Y > ViewPlinePts[2].Y)
                {
                    baseHeight = ViewPlinePts[2].Y;
                }
                Point2d ViewCenterPt = new Point2d(
                    baseWidth + (Math.Abs(ViewPlinePts[0].X - ViewPlinePts[2].X)/2),
                    baseHeight + (Math.Abs(ViewPlinePts[0].Y - ViewPlinePts[2].Y)/2));

                #endregion

                #region Select Paper Space

                PromptKeywordOptions PaperSpaceOpts = new PromptKeywordOptions("");
                PaperSpaceOpts.Message = "\nChose paper space : ";
                PaperSpaceOpts.AllowNone = false;

                Layout currLayout;
                // Open the Block table for read
                BlockTable Bt = transaction.GetObject(_doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord BtrModelSpace =
                    (BlockTableRecord) transaction.GetObject(Bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId BtrId in Bt)
                {
                    BlockTableRecord Btr = (BlockTableRecord) transaction.GetObject(BtrId, OpenMode.ForRead);
                    if (Btr.IsLayout && Btr.ObjectId != BtrModelSpace.ObjectId)
                    {
                        currLayout = (Layout) transaction.GetObject(Btr.LayoutId, OpenMode.ForRead);
                        PaperSpaceOpts.Keywords.Add(currLayout.LayoutName);
                    }
                }
                PromptResult PaperSpaceRes = _doc.Editor.GetKeywords(PaperSpaceOpts);
                LayoutManager.Current.CurrentLayout = PaperSpaceRes.StringResult;

                #endregion

                var keywordSelectionResult =
                    _editorHelper.PromptForKeywordSelection(
                        "\nSpecify rectangle describing viewport or base point with scale factor",
                        new[] {"Rectangle", "Base point"},
                        false,
                        "Rectangle");
                if (keywordSelectionResult.Status != PromptStatus.OK)
                {
                    return;
                }

                var viewportHeight = 0.0;
                var viewportWidth = 0.0;
                var CenterPt = new Point3d();
                var viewportCustomScale = 0.0;

                if (keywordSelectionResult.StringResult == "Rectangle")
                {
                    #region Get Viewport Rectangle Pts

                    VportRectangleOpts = new PromptPointOptions("\nSpecify rectangle describing the viewport : \n");
                    VportRectangleRes = _doc.Editor.GetPoint(VportRectangleOpts);

                    if (VportRectangleRes.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    CornerPt = VportRectangleRes.Value;
                    Recjig = new RectangleJig(CornerPt);
                    if (_doc.Editor.Drag(Recjig).Status == PromptStatus.Cancel)
                    {
                        return;
                    }

                    Polyline ViewportPline = Recjig.Polyline;
                    Point3dCollection ViewportPlinePts = GeometryUtility.GetPointsFromPolyline(ViewportPline);
                    GeometryUtility.RemoveDuplicate(ViewportPlinePts);

                    if (ViewportPlinePts.Count == 5)
                    {
                        ViewportPlinePts.RemoveAt(4);
                    }

                    #endregion

                    #region Calculate Viewport height

                    viewportHeight = Math.Abs(ViewportPlinePts[0].Y - ViewportPlinePts[2].Y);

                    #endregion

                    #region Calculate Viewport width

                    viewportWidth = Math.Abs(ViewportPlinePts[0].X - ViewportPlinePts[2].X);

                    #endregion

                    #region Calculate Viewport Center Pt

                    baseWidth = ViewportPlinePts[0].X;
                    if (ViewportPlinePts[0].X > ViewportPlinePts[2].X)
                    {
                        baseWidth = ViewportPlinePts[2].X;
                    }

                    baseHeight = ViewportPlinePts[0].Y;
                    if (ViewportPlinePts[0].Y > ViewportPlinePts[2].Y)
                    {
                        baseHeight = ViewportPlinePts[2].Y;
                    }
                    CenterPt = new Point3d(
                        baseWidth + (Math.Abs(ViewportPlinePts[0].X - ViewportPlinePts[2].X)/2),
                        baseHeight + (Math.Abs(ViewportPlinePts[0].Y - ViewportPlinePts[2].Y)/2),
                        0.0);

                    #endregion

                    #region Calculate Viewport Custom Scale

                    viewportCustomScale = viewportHeight/viewHeight;

                    #endregion
                }
                else
                {
                    #region Get Viewport Base Point

                    var viewportBasePoint = _editorHelper.PromptForPoint("\nSpecify bottom left point of viewport : ");
                    if (viewportBasePoint.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    #endregion

                    #region Get ViewportCustomScale

                    var scaleFactorResult = _editorHelper.PromptForDouble("\nSpecify viewport scale factor : ", 1.0);
                    if (scaleFactorResult.Status != PromptStatus.OK)
                    {
                        return;
                    }
                    if (scaleFactorResult.Value <= 0)
                    {
                        _editorHelper.WriteMessage("\nSpecified scale factor is incorrect !\n");
                    }
                    viewportCustomScale = scaleFactorResult.Value;

                    #endregion

                    #region Calculate Viewport height

                    viewportHeight = viewHeight*viewportCustomScale;

                    #endregion

                    #region Calculate Viewport width

                    viewportWidth = viewWidth*viewportCustomScale;

                    #endregion

                    #region Calculate Viewport Center Pt

                    CenterPt = new Point3d(viewportBasePoint.Value.X + viewportWidth/2,
                        viewportBasePoint.Value.Y + viewportHeight/2, 0);

                    #endregion
                }

                #region create viewport

                var Vport = new Viewport
                {
                    ViewCenter = ViewCenterPt,
                    CenterPoint = CenterPt,
                    Height = viewportHeight,
                    Width = viewportWidth,
                    CustomScale = viewportCustomScale
                };

                #endregion

                #region Insert viewport into paper space

                BlockTableRecord PaperSpaceBtr = new BlockTableRecord();
                foreach (ObjectId BtrId in Bt)
                {
                    BlockTableRecord Btr = (BlockTableRecord) transaction.GetObject(BtrId, OpenMode.ForRead);
                    if (Btr.IsLayout && Btr.ObjectId != BtrModelSpace.ObjectId)
                    {
                        currLayout = (Layout) transaction.GetObject(Btr.LayoutId, OpenMode.ForRead);
                        if (currLayout.LayoutName == PaperSpaceRes.StringResult)
                        {
                            PaperSpaceBtr =
                                (BlockTableRecord)
                                    transaction.GetObject(Bt[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
                        }
                    }
                }

                if (PaperSpaceBtr == null)
                {
                    _doc.Editor.WriteMessage("Error getting the selected layout");
                }
                // Add the new object to the block table record and the transaction
                PaperSpaceBtr.AppendEntity(Vport);

                transaction.AddNewlyCreatedDBObject(Vport, true);

                // Enable the viewport
                Vport.On = true;

                // Set the new viewport current via an imported ObjectARX function
#if acad2012

                SetCurrentVPort2010(Vport.UnmanagedObject);
#endif
#if acad2013

                SetCurrentVPort2013(Vport.UnmanagedObject);
#endif
#if bcad

                SetCurrentVPortBCad(Vport.UnmanagedObject);
#endif

                #endregion

                transaction.Commit();
            }
        }

        /// <summary>
        /// Lock all Viewports on entire DWG
        /// </summary>
        [CommandMethod("vlockall")]
        public void LockAllViewportsStart()
        {
            

            TypedValue[] viewportFilter = { new TypedValue((int)DxfCode.Start, "Viewport") };
            try
            {
                PromptSelectionResult viewportSelection =
                    Application.DocumentManager.MdiActiveDocument.Editor.SelectAll(new SelectionFilter(viewportFilter));
                SelectionSet selectionSet = viewportSelection.Value;
                using (var transaction = _doc.Database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objectId in selectionSet.GetObjectIds())
                    {
                        Viewport viewport = (Viewport)transaction.GetObject(objectId, OpenMode.ForWrite);
                        viewport.Locked = true;
                    }
                    transaction.Commit();
                }
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Unlock all Viewports on entire DWG
        /// </summary>
        [CommandMethod("vunlockall")]
        public void UnlockAllViewportsStart()
        {
            

            TypedValue[] viewportFilter = { new TypedValue((int)DxfCode.Start, "Viewport") };
            try
            {
                PromptSelectionResult viewportSelection =
                    Application.DocumentManager.MdiActiveDocument.Editor.SelectAll(new SelectionFilter(viewportFilter));
                SelectionSet selectionSet = viewportSelection.Value;
                using (var transaction = _doc.Database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objectId in selectionSet.GetObjectIds())
                    {
                        Viewport viewport = (Viewport)transaction.GetObject(objectId, OpenMode.ForWrite);
                        viewport.Locked = false;
                    }
                    transaction.Commit();
                }
            }
            catch (Exception)
            {
            }

        }
    }
}
