using System;
using Castle.Core.Logging;
using KojtoCAD.Mathematics.Geometry;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using KojtoCAD.BlockItems.Interfaces;
using System.Reflection;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
using Exception = Teigha.Runtime.Exception;
#endif
[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.Insulations.RigidInsulation))]

namespace KojtoCAD.GraphicItems.Insulations
{
    public class RigidInsulation
    {
        private const double HalfPi = 1.5707963267949;
        private readonly Database _db = Application.DocumentManager.MdiActiveDocument.Database;
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public RigidInsulation()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
        }

        /// <summary>
        /// Rigid Insulation
        /// </summary>
        [CommandMethod("ri")]
        public void RigidInsulationStart()
        {
            var promptPointOptions = new PromptPointOptions("\nSelect First Point or [Block]", "Block");
            var ppr = _editorHelper.PromptForPoint(promptPointOptions);

            switch (ppr.Status)
            {
                case PromptStatus.OK: DrawByLineAndThickness(ppr.Value); break;
                case PromptStatus.Keyword: ImportInsulationBlock(); break;
                default: break;
            }
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }

        private void ImportInsulationBlock()
        {
            var insertionPointResult = _editorHelper.PromptForPoint("Pick insertion point : ");
            if (insertionPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            var dynamicBlockPath = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockPath == null)
            {
                _editorHelper.WriteMessage("Dynamic block RigidInsulation.dwg does not exist.");
                return;
            }
            _drawingHelper.ImportDynamicBlockAndFillItsProperties(
               dynamicBlockPath, insertionPointResult.Value, new System.Collections.Hashtable { { "Distance", 10d } }, new System.Collections.Hashtable());
        }

        private void DrawByLineAndThickness(Point3d firstPoint)
        {
            double thicknessAtFirstPoint = Settings.Default.RInsulationThicknessAtFirstPoint;
            double thicknessAtSecondPoint = Settings.Default.RInsulationThicknessAtSecondPoint;

            var secondPointResult = _editorHelper.PromptForPoint("Select second point : ", true, true, firstPoint);
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var thicknessAtFirstPointResult = _editorHelper.PromptForDouble("Set thickness at first point : ", thicknessAtFirstPoint);
            if (thicknessAtFirstPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            thicknessAtFirstPoint = thicknessAtFirstPointResult.Value;
            var thicknessAtSecondPointResult = _editorHelper.PromptForDouble("Set thickness at second point : ", thicknessAtSecondPoint);
            if (thicknessAtSecondPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            thicknessAtSecondPoint = thicknessAtSecondPointResult.Value;
            Settings.Default.RInsulationThicknessAtFirstPoint = thicknessAtFirstPoint;
            Settings.Default.RInsulationThicknessAtSecondPoint = thicknessAtSecondPoint;
            Settings.Default.Save();

            DrawRigidInsulation(firstPoint, secondPointResult.Value, thicknessAtFirstPoint, thicknessAtSecondPoint);
        }

        private void DrawRigidInsulation(Point3d aFirstPoint, Point3d aSecondPoint, double aThicknessAtFirstPoint, double aThicknessAtSecondPoint)
        {
            double angleFirstToSecondPoint = GeometryUtility.GetAngleFromXAxis(aFirstPoint, aSecondPoint);

            Point3d thirdPoint = GeometryUtility.GetPlanePolarPoint(aFirstPoint, angleFirstToSecondPoint + HalfPi, aThicknessAtFirstPoint);
            Point3d fourthPoint = GeometryUtility.GetPlanePolarPoint(aSecondPoint, angleFirstToSecondPoint + HalfPi, aThicknessAtSecondPoint);

            double angleThirdToFourthPoint = GeometryUtility.GetAngleFromXAxis(thirdPoint, fourthPoint);

            double distFirstToSecondPoint = aFirstPoint.DistanceTo(aSecondPoint);
            double distThirdToFourthPoint = thirdPoint.DistanceTo(fourthPoint);

            double midThickness = (aThicknessAtFirstPoint + aThicknessAtSecondPoint) / 2;
            int segmentsCount = (int)Math.Ceiling((distFirstToSecondPoint / midThickness) + 0.5) * 4;

            double lowerSegmentLenght = distFirstToSecondPoint / segmentsCount;
            double upperSegmentLenght = distThirdToFourthPoint / segmentsCount;

            Point3dCollection rigidInsulationPolyPts = new Point3dCollection();

            // loop for the upper and lower points
            int i;
            for (i = 1; i <= segmentsCount; i++)
            {
                Point3d nextPoint;
                if (i % 2 == 0)
                {
                    // It's even --> Lower Segment Point
                    nextPoint = GeometryUtility.GetPlanePolarPoint(aFirstPoint, angleFirstToSecondPoint, (i - 0.5) * lowerSegmentLenght);
                }
                else
                {
                    // It's odd --> Upper Segment Point
                    nextPoint = GeometryUtility.GetPlanePolarPoint(thirdPoint, angleThirdToFourthPoint, (i - 0.5) * upperSegmentLenght);
                }
                rigidInsulationPolyPts.Add(nextPoint);
            }

            //Add the last point if we finish on the lower segment.
            if (i % 2 == 0)
            {
                rigidInsulationPolyPts.Add(aSecondPoint);
            }

            // closing the gaps of the polyline    
            rigidInsulationPolyPts.Add(fourthPoint);
            rigidInsulationPolyPts.Add(thirdPoint);

            Polyline3d rigidInsulationPline = new Polyline3d(Poly3dType.SimplePoly, rigidInsulationPolyPts, true);

            Point3dCollection rigidInsulationCounturPts = new Point3dCollection();
            rigidInsulationCounturPts.Add(aFirstPoint);
            rigidInsulationCounturPts.Add(aSecondPoint);
            rigidInsulationCounturPts.Add(fourthPoint);
            rigidInsulationCounturPts.Add(thirdPoint);
            rigidInsulationCounturPts.Add(aFirstPoint);
            Polyline3d rigidInsulationCountur = new Polyline3d(Poly3dType.SimplePoly, rigidInsulationCounturPts, true);

            try
            {
                using (var transaction = _db.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(_db.BlockTableId, OpenMode.ForWrite);

                    var oldLayer = _db.Clayer;

                    _drawingHelper.LayerManipulator.CreateLayer("3-0", System.Drawing.Color.Lime);

                    var rigidInsulation = new BlockTableRecord();
                    var nameSalt = DateTime.Now.GetHashCode().ToString();
                    rigidInsulation.Name = "RigidInsulation_" + nameSalt;
                    rigidInsulation.Origin = aFirstPoint;

                    rigidInsulationPline.Layer = "0";
                    rigidInsulationCountur.Layer = "0";

                    blockTable.Add(rigidInsulation);
                    transaction.AddNewlyCreatedDBObject(rigidInsulation, true);

                    rigidInsulation.AppendEntity(rigidInsulationPline);
                    transaction.AddNewlyCreatedDBObject(rigidInsulationPline, true);

                    rigidInsulation.AppendEntity(rigidInsulationCountur);
                    transaction.AddNewlyCreatedDBObject(rigidInsulationCountur, true);

                    var rigidInsulationRef = new BlockReference(aFirstPoint, rigidInsulation.ObjectId) { Layer = "3-0" };
                    rigidInsulationRef.TransformBy(_editorHelper.CurrentUcs);

                    var currentSpace = (BlockTableRecord)transaction.GetObject(_db.CurrentSpaceId, OpenMode.ForWrite);

                    currentSpace.AppendEntity(rigidInsulationRef);
                    transaction.AddNewlyCreatedDBObject(rigidInsulationRef, true);
                    _drawingHelper.LayerManipulator.ChangeLayer(oldLayer);
                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                _logger.Error("Error.", exception);
            }
        }
    }
}
