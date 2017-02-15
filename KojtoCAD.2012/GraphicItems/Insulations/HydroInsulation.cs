using Castle.Core.Logging;
using KojtoCAD.Mathematics.Geometry;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;

#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Teigha.Colors;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
using Color = Teigha.Colors.Color;
#endif

[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.Insulations.HydroInsulation))]

namespace KojtoCAD.GraphicItems.Insulations
{
    public class HydroInsulation
    {
        private ObjectId _selectedObjectId;
        private Point3d _sidePoint;
        private double _internalRadius;
        private double _thickness;
        private double _segmentLength;
        
        private ObjectId _layerOfContour;
        private ObjectId _layerOfSegments;

        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger;

        public HydroInsulation()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Hydro Insulation
        /// </summary>
        [CommandMethod("hi")]
        public void HydroInsulationStart()
        {
            
            var keywords = new[] {"Points", "polYline"};
            var insulationModeResult = _editorHelper.PromptForKeywordSelection("HidroInsulation over points or object",
                keywords, false, "Points");
            if (insulationModeResult.Status != PromptStatus.OK)
            {
                return;
            }
            using (var tr = _drawingHelper.TransactionManager.StartTransaction())
            {
                switch (insulationModeResult.StringResult)
                {
                    case "Points":
                        var insulationVertices = GetPointsFromUser();
                        if (insulationVertices.Count < 2)
                        {
                            return;
                        }
                        var insulationBaseLine = new Polyline();
                        insulationBaseLine.SetDatabaseDefaults();
                        insulationBaseLine.Color = Color.FromColor(System.Drawing.Color.Yellow);
                        var counter = 0;
                        foreach (Point3d insulationVertex in insulationVertices)
                        {
                            insulationBaseLine.AddVertexAt(counter, new Point2d(insulationVertex.X, insulationVertex.Y),
                                0, 0, 0);
                            counter++;
                        }
                        AddPolylineToModelSpace(insulationBaseLine, tr);
                        _selectedObjectId = insulationBaseLine.ObjectId;

                        break;

                    case "polYline":
                        var promptEntityResult = _editorHelper.PromptForObject(
                            "\nSelect the Polyline : ", typeof (Polyline), true);
                        if (promptEntityResult.Status != PromptStatus.OK)
                        {
                            return;
                        }
                        _selectedObjectId = promptEntityResult.ObjectId;
                        break;
                }

                var sidePointResult = _editorHelper.PromptForPoint("Select side : ");
                if (sidePointResult.Status != PromptStatus.OK)
                {
                    RemovePolylineFromModelSpace(_selectedObjectId, tr);
                    return;
                }
                _sidePoint = sidePointResult.Value;
                var thicknessResult = _editorHelper.PromptForDouble("Set thickness : ",
                    Settings.Default.HInsulationThickness);
                if (thicknessResult.Status != PromptStatus.OK)
                {
                    RemovePolylineFromModelSpace(_selectedObjectId, tr);
                    return;
                }
                _thickness = thicknessResult.Value;
                Settings.Default.HInsulationThickness = _thickness;


                var internalRadiusResult = _editorHelper.PromptForDouble("Set internal radius : ",
                    Settings.Default.HInsulationInternalRadius);
                if (internalRadiusResult.Status != PromptStatus.OK)
                {
                    RemovePolylineFromModelSpace(_selectedObjectId, tr);
                    return;
                }
                _internalRadius = internalRadiusResult.Value;
                Settings.Default.HInsulationInternalRadius = _internalRadius;

                var segmentLengthResult = _editorHelper.PromptForDouble("Set segment length : ",
                    Settings.Default.HInsulationSegmentLenght);
                if (segmentLengthResult.Status != PromptStatus.OK)
                {
                    RemovePolylineFromModelSpace(_selectedObjectId, tr);
                    return;
                }
                _segmentLength = segmentLengthResult.Value;
                Settings.Default.HInsulationSegmentLenght = _segmentLength;

                Settings.Default.Save();

                CreateHydroInsulationLayers();
                CreateHydroInsulation();
                tr.Commit();
            }
            _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        private Point3dCollection GetPointsFromUser()
        {
            var vertices = new Point3dCollection();
            var verticesRslt = _editorHelper.PromptForPoint("\nSelect point: ");
            while (verticesRslt.Status == PromptStatus.OK)
            {
                vertices.Add(verticesRslt.Value);
                verticesRslt = _editorHelper.PromptForPoint("\nSelect point: ",true,true,verticesRslt.Value);
                if (verticesRslt.Status == PromptStatus.OK)
                {
                    _editorHelper.DrawVector(vertices[vertices.Count - 1], verticesRslt.Value, Color.FromColor(System.Drawing.Color.Yellow).ColorIndex, false);
                }
            }
            return vertices;
        }

        private void AddPolylineToModelSpace(Entity polyline, Transaction tr)
        {
            var blockTable = tr.GetObject(_drawingHelper.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
            var modelSpace = (BlockTableRecord) tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            modelSpace.AppendEntity(polyline);
            tr.AddNewlyCreatedDBObject(polyline, true);
        }

        private void RemovePolylineFromModelSpace(ObjectId entityId, Transaction tr)
        {
            var entitiesForErasing = new ObjectIdCollection();

            var polylineForErasing = tr.GetObject(entityId, OpenMode.ForWrite) as Polyline;
            if (polylineForErasing == null)
            {
                return;
            }
            entitiesForErasing.Add(entityId);
            polylineForErasing.Erase();
            _drawingHelper.Database.ReclaimMemoryFromErasedObjects(entitiesForErasing);
        }

        private void CreateHydroInsulation()
        {
            CreateHydroInsulationContour(_selectedObjectId, _thickness, _internalRadius);

            using (var transaction = _drawingHelper.TransactionManager.StartTransaction())
            {
                var lineTypeSourceFile = new UtilityClass().GetDefaultLinetypeFile();
                _drawingHelper.LayerManipulator.SetLayerLinetype("1-1", "DASHED", lineTypeSourceFile);

                var firstPoly = (Polyline) transaction.GetObject(_selectedObjectId, OpenMode.ForWrite);
                firstPoly.LayerId = _layerOfContour;
                firstPoly.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);

                var middlePoly =
                    (Polyline)
                        GeometryUtility.ObjOffset(_selectedObjectId,
                            _thickness*0.5*GeometryUtility.OffsetInDirection(_selectedObjectId, _sidePoint), transaction)
                            [0];
                middlePoly.LayerId = _layerOfSegments;
                middlePoly.ConstantWidth = _thickness;
                middlePoly.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);
                /// TODO : Should implement algorithm for proper linetypscale
                middlePoly.LinetypeScale = 10.0;

                var lastPoly =
                    (Polyline)
                        GeometryUtility.ObjOffset(_selectedObjectId,
                            _thickness*GeometryUtility.OffsetInDirection(_selectedObjectId, _sidePoint), transaction)[
                                0];
                lastPoly.LayerId = _layerOfContour;
                lastPoly.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);

                transaction.Commit();
            }
        }

        private static void CreateHydroInsulationContour(ObjectId objectId, double thickness, double internalRadius)
        {
            CommandLineHelper.Command("._FILLET", "_R", internalRadius + thickness / 2);
            CommandLineHelper.Command("._FILLET", "_P", objectId);
        }

        private void CreateHydroInsulationLayers()
        {
            _layerOfContour = _drawingHelper.LayerManipulator.CreateLayer("2-0", System.Drawing.Color.Yellow);
            _layerOfSegments = _drawingHelper.LayerManipulator.CreateLayer("1-1", System.Drawing.Color.Cyan);
        }
    }
}
