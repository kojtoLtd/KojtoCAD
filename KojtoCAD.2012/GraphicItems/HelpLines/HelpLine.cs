using Castle.Core.Logging;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
using Exception = Teigha.Runtime.Exception;
#endif

[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.HelpLines.HelpLine))]

namespace KojtoCAD.GraphicItems.HelpLines
{
    public class HelpLine
    {
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly EditorHelper _editorHelper;
        private readonly Document _doc = Application.DocumentManager.MdiActiveDocument;

        public HelpLine()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Horizontal help line
        /// </summary>
        [CommandMethod("hh")]
        public void HelpLineHorizontalStart()
        {
            CreateHelpLine("Horizontal");
        }

        /// <summary>
        /// Vertical help line
        /// </summary>
        [CommandMethod("hv")]
        public void HelpLineVerticalStart()
        {
            CreateHelpLine("Vertical");
        }

        /// <summary>
        /// Crossed help line
        /// </summary>
        [CommandMethod("hc")]
        public void HelpLineCrossedStart()
        {
            CreateHelpLine("Crossed");
        }

        /// <summary>
        /// Help line with custom angle
        /// </summary>
        [CommandMethod("ha")]
        public void HelpLineAngularStart()
        {
            CreateHelpLine("Angular");
        }

        /// <summary>
        /// Help Line 45
        /// </summary>
        [CommandMethod("h45")]
        public void HelpLine45Start()
        {
            CreateHelpLine("Deg45");
        }

        /// <summary>
        /// Help line on offset distance
        /// </summary>
        [CommandMethod("xf")]
        public void HelpLineOffsetStart()
        {
            _doc.SendStringToExecute("xl ", true, false, true);
            _doc.SendStringToExecute("offset ", true, false, true);
        }

        /// <summary>
        /// Delete help line
        /// </summary>
        [CommandMethod("hd")]
        public void HelpLinesDeleteStart()
        {
            TypedValue[] acTypValAr = new TypedValue[ 1 ];
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "XLINE"), 0);
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            var objectIdCollectionResult = _editorHelper.PromptForSelection("", acSelFtr);
            if ( objectIdCollectionResult.Status != PromptStatus.OK )
            {
                return;
            }

            using ( var acTrans = _doc.Database.TransactionManager.StartTransaction() )
            {
                foreach ( var objectId in objectIdCollectionResult.Value.GetObjectIds() )
                {
                    objectId.GetObject(OpenMode.ForWrite).Erase();
                }
                acTrans.Commit();
            }
        }

        private void CreateHelpLine(string lineType)
        {
            Xline helpLine = new Xline();
            Xline helpLineCross = new Xline();

            if ( string.IsNullOrEmpty(lineType) || string.IsNullOrWhiteSpace(lineType) )
            {
                return;
            }

            ObjectId layerHelp = _drawingHelper.LayerManipulator.CreateLayer("HELP", System.Drawing.Color.Gray);
            var basePointResult = _editorHelper.PromptForPoint("Pick insertion point :");
            if ( basePointResult.Status != PromptStatus.OK )
            {
                return;
            }
            helpLine.BasePoint = basePointResult.Value;
            //  detect the selected line type
            switch ( lineType )
            {
                case "Horizontal":
                    helpLine.UnitDir = Vector3d.XAxis;
                    break;
                case "Vertical":
                    helpLine.UnitDir = Vector3d.YAxis;
                    break;
                case "Crossed":

                    helpLine.UnitDir = Vector3d.YAxis;
                    helpLineCross.BasePoint = helpLine.BasePoint;
                    helpLineCross.UnitDir = Vector3d.XAxis;
                    break;
                case "Angular":
                    var directinPointResult = _editorHelper.PromptForPoint("Pick direction point :");
                    if ( directinPointResult.Status != PromptStatus.OK )
                    {
                        return;
                    }

                    helpLine.UnitDir = new Vector3d(helpLine.BasePoint.X - directinPointResult.Value.X,
                                                    helpLine.BasePoint.Y - directinPointResult.Value.Y,
                                                    helpLine.BasePoint.Z - directinPointResult.Value.Z);
                    break;
                case "Deg45":
                    Vector3d Vector45 = new Vector3d(5, 5, 0);
                    helpLine.UnitDir = Vector45;
                    break;

            }

            try
            {
                using ( Transaction transaction = _doc.Database.TransactionManager.StartTransaction() )
                {
                    //  open current space (model space in our case) for write
                    BlockTableRecord btr =
                        (BlockTableRecord) transaction.GetObject(_doc.Database.CurrentSpaceId, OpenMode.ForWrite);

                    //  set Xline Layer
                    helpLine.SetLayerId(layerHelp, false);
                    Matrix3d mat = _editorHelper.CurrentUcs;
                    helpLine.TransformBy(mat);
                    btr.AppendEntity(helpLine);

                    //  add the Xline to the transaction
                    transaction.AddNewlyCreatedDBObject(helpLine, true);

                    if ( ( lineType == "Crossed" ) )
                    {
                        //  set Xline Layer
                        helpLineCross.TransformBy(mat);
                        helpLineCross.SetLayerId(layerHelp, false);
                        btr.AppendEntity(helpLineCross);
                        //  add the Xline to the transaction
                        transaction.AddNewlyCreatedDBObject(helpLineCross, true);
                    }
                    transaction.Commit();
                }
            }
            catch ( Exception exception )
            {
                _logger.Error("Help Line error", exception);
            }

        }
    }
}
