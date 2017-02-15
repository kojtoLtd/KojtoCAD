using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using System.Collections.Generic;
using System.Windows.Forms;
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

[assembly : CommandClass(typeof(KojtoCAD.LayoutCommands.LayoutSplitter))]

namespace KojtoCAD.LayoutCommands
{
    public class LayoutSplitter
    {
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        /// <summary>
        /// Layout Splitter
        /// </summary>
        [CommandMethod("LayoutToFile_2012", "DV", null, CommandFlags.Modal, null, "LayoutToFile_2012", "LayoutToFile_2012")]
        public void StartLayoutSplitterStart()
        {
            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


            Application.SetSystemVariable("TILEMODE", 1);
            Zoom(new Point3d(), new Point3d(), new Point3d(), 1.01075);
            Application.SetSystemVariable("SAVEFIDELITY", 0);
            Application.SetSystemVariable("REGENMODE", 0);
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            PromptStringOptions prefixOptions = new PromptStringOptions("Enter Prefix :");
            PromptResult prefixResult = ed.GetString(prefixOptions);
            if ( prefixResult.Status != PromptStatus.OK )
            {
                return;
            }
            PromptStringOptions sufixOptions = new PromptStringOptions("\n\rEnter Sufix :");
            PromptResult sufixResult = ed.GetString(sufixOptions);
            if ( sufixResult.Status != PromptStatus.OK )
            {
                return;
            }

            string selectedFolder = GetFolder();// Името на каталога в който ще се запсват файловете
            if ( selectedFolder.Length < 2 ) { return; }
            selectedFolder = selectedFolder.Replace(@"\", @"\\");
            selectedFolder += prefixResult.StringResult;

            Application.MainWindow.Visible = false;

            try
            {

                List<Pair<Pair<string, ObjectId>, ObjectIdCollection>> obcLIST = new
               List<Pair<Pair<string, ObjectId>, ObjectIdCollection>>();
                #region прочита лейутите, селектира обектите от виевпортовете и ги запазва в масив
                using ( doc.LockDocument() )
                {
                    using ( Transaction transaction = doc.TransactionManager.StartTransaction() )
                    {
                        BlockTable blockTable = transaction.GetObject(db.BlockTableId,                OpenMode.ForRead) as BlockTable;

                        // Open the Block table record Model space for write
                        BlockTableRecord blockTableRecord = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;


                        List<Pair<Pair<string, ObjectId>, List<Pair<Point3d, Point3d>>>> list = LayoutHelper.CreateArrayOfLayoutNamesAndViewportsDiagonals();
                        Application.SetSystemVariable("TILEMODE", 1);
                        foreach ( Pair<Pair<string, ObjectId>, List<Pair<Point3d, Point3d>>> la in list )
                        {
                            ObjectIdCollection objcoll = new ObjectIdCollection();
                            foreach ( Pair<Point3d, Point3d> diagonal in la.Second )
                            {
                                PromptSelectionResult prrr = ed.SelectCrossingWindow(diagonal.First, diagonal.Second);
                                if ( prrr.Status == PromptStatus.OK )
                                {
                                    SelectionSet acSSett = prrr.Value;
                                    ObjectIdCollection acObjIdColl = new ObjectIdCollection(acSSett.GetObjectIds());
                                    if ( ( acObjIdColl != null ) && ( acObjIdColl.Count > 0 ) )
                                        foreach ( ObjectId iid in acObjIdColl )
                                        {
                                            objcoll.Add(iid);
                                        }

                                }
                            }
                            if ( objcoll.Count > 0 )
                                obcLIST.Add(new Pair<Pair<string, ObjectId>, ObjectIdCollection>(la.First, objcoll));



                        }
                        transaction.Commit();
                    }
                }// 
                #endregion

                ObjectIdCollection kju = new ObjectIdCollection();
                #region прочитане на обектите от моделното пространство и складира ObjectIds в масив
                using ( DocumentLock DocLock = doc.LockDocument() )
                {
                    using ( Transaction acTrans = db.TransactionManager.StartTransaction() )
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = acTrans.GetObject(db.BlockTableId,
                                                     OpenMode.ForRead) as BlockTable;

                        // Open the Block table record Model space for write
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                        OpenMode.ForWrite) as BlockTableRecord;
                        foreach ( ObjectId ig in acBlkTblRec )
                        {
                            kju.Add(ig);
                        }
                    }
                }
                #endregion


                #region export
                LayoutManager acLayoutMgr;
                acLayoutMgr = LayoutManager.Current;
                using ( DocumentLock DocLock = doc.LockDocument() )
                {
                    foreach ( Pair<Pair<string, ObjectId>, ObjectIdCollection> item in obcLIST )
                    {
                        using ( Transaction acTrans = db.TransactionManager.StartTransaction() )
                        {
                            foreach ( ObjectId ig in kju )
                            {
                                bool exist = false;
                                foreach ( ObjectId igg in item.Second )
                                {
                                    if ( igg == ig )
                                    {
                                        exist = true;
                                        break;
                                    }
                                }
                                if ( !exist )
                                {
                                    Entity ent = null;
                                    try
                                    {
                                        ent = acTrans.GetObject(ig, OpenMode.ForWrite) as Entity;

                                    }
                                    catch { }
                                    if ( ent != null )
                                        ent.Erase();
                                }
                            }

                            foreach ( Pair<Pair<string, ObjectId>, ObjectIdCollection> it in obcLIST )
                            {
                                if ( it.First.First != item.First.First )
                                {
                                    acLayoutMgr.DeleteLayout(it.First.First);
                                }

                            }
                            try
                            {
                                db.SaveAs(selectedFolder + item.First.First + sufixResult.StringResult + ".DWG", DwgVersion.Current);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                #endregion

                // MgdAcApplication.MainWindow.WindowState = Autodesk.AutoCAD.Windows.Window.State.Maximized;
                // MgdAcApplication.MainWindow.Visible = false;
            }
            catch
            {
                Application.MainWindow.Visible = true;
            }
            Application.MainWindow.Visible = true;
        }


        private string GetFolder()
        {
            string SelectedFolder = "";
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if ( folderBrowserDialog.ShowDialog() == DialogResult.OK )
            {
                SelectedFolder = folderBrowserDialog.SelectedPath;
            }
            return SelectedFolder + "\\";
        }
        static void Zoom(Point3d pMin, Point3d pMax, Point3d pCenter, double dFactor)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            int nCurVport = System.Convert.ToInt32(Application.GetSystemVariable("CVPORT"));

            // Get the extents of the current space no points 
            // or only a center point is provided
            // Check to see if Model space is current
            if ( acCurDb.TileMode == true )
            {
                if ( pMin.Equals(new Point3d()) == true &&
                    pMax.Equals(new Point3d()) == true )
                {
                    pMin = acCurDb.Extmin;
                    pMax = acCurDb.Extmax;
                }
            }
            else
            {
                // Check to see if Paper space is current
                if ( nCurVport == 1 )
                {
                    // Get the extents of Paper space
                    if ( pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true )
                    {
                        pMin = acCurDb.Pextmin;
                        pMax = acCurDb.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if ( pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true )
                    {
                        pMin = acCurDb.Extmin;
                        pMax = acCurDb.Extmax;
                    }
                }
            }

            // Start a transaction
            using ( Transaction acTrans = acCurDb.TransactionManager.StartTransaction() )
            {
                // Get the current view
                using ( ViewTableRecord acView = acDoc.Editor.GetCurrentView() )
                {
                    Extents3d eExtents;

                    // Translate WCS coordinates to DCS
                    Matrix3d matWCS2DCS;
                    matWCS2DCS = Matrix3d.PlaneToWorld(acView.ViewDirection);
                    matWCS2DCS = Matrix3d.Displacement(acView.Target - Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = Matrix3d.Rotation(-acView.ViewTwist,
                                                   acView.ViewDirection,
                                                   acView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max 
                    // point of the extents
                    // for Center and Scale modes
                    if ( pCenter.DistanceTo(Point3d.Origin) != 0 )
                    {
                        pMin = new Point3d(pCenter.X - ( acView.Width / 2 ),
                                           pCenter.Y - ( acView.Height / 2 ), 0);

                        pMax = new Point3d(( acView.Width / 2 ) + pCenter.X,
                                           ( acView.Height / 2 ) + pCenter.Y, 0);
                    }

                    // Create an extents object using a line
                    using ( Line acLine = new Line(pMin, pMax) )
                    {
                        eExtents = new Extents3d(acLine.Bounds.Value.MinPoint,
                                                 acLine.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double dViewRatio;
                    dViewRatio = ( acView.Width / acView.Height );

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    eExtents.TransformBy(matWCS2DCS);

                    double dWidth;
                    double dHeight;
                    Point2d pNewCentPt;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if ( pCenter.DistanceTo(Point3d.Origin) != 0 )
                    {
                        dWidth = acView.Width;
                        dHeight = acView.Height;

                        if ( dFactor == 0 )
                        {
                            pCenter = pCenter.TransformBy(matWCS2DCS);
                        }

                        pNewCentPt = new Point2d(pCenter.X, pCenter.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        dWidth = eExtents.MaxPoint.X - eExtents.MinPoint.X;
                        dHeight = eExtents.MaxPoint.Y - eExtents.MinPoint.Y;

                        // Get the center of the view
                        pNewCentPt = new Point2d(( ( eExtents.MaxPoint.X + eExtents.MinPoint.X ) * 0.5 ),
                                                 ( ( eExtents.MaxPoint.Y + eExtents.MinPoint.Y ) * 0.5 ));
                    }

                    // Check to see if the new width fits in current window
                    if ( dWidth > ( dHeight * dViewRatio ) ) dHeight = dWidth / dViewRatio;

                    // Resize and scale the view
                    if ( dFactor != 0 )
                    {
                        acView.Height = dHeight * dFactor;
                        acView.Width = dWidth * dFactor;
                    }

                    // Set the center of the view
                    acView.CenterPoint = pNewCentPt;

                    // Set the current view
                    acDoc.Editor.SetCurrentView(acView);
                }

                // Commit the changes
                acTrans.Commit();
            }
        }
    }
}
