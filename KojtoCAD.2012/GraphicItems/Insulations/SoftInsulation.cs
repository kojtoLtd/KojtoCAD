using System.Collections.Generic;
using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using System;
using KojtoCAD.Mathematics.Geometry;
using Line2d = KojtoCAD.Mathematics.Geometry.KLine2D;
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
[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.Insulations.SoftInsulation))]

namespace KojtoCAD.GraphicItems.Insulations
{
    public class SoftInsulation
    {
        private static ILogger _logger = NullLogger.Instance;
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _documentHelper;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public static ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public SoftInsulation()
        {
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _documentHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
        }

        [CommandMethod("si")]
        public void SoftInsulationStart()
        {

            var promptPointOptions = new PromptPointOptions("\nSelect First Point or [3points/4points/Polyline/Block]",
                                                            "3points 4points Polyline Block");
            var ppr = _editorHelper.PromptForPoint(promptPointOptions);
            if (ppr.Status == PromptStatus.Keyword || ppr.Status == PromptStatus.OK)
            {
                switch (ppr.StringResult)
                {
                    case "3points":
                        DrawInsulationOnThreePoints();
                        break;
                    case "4points":
                        DrawInsulationOnFourPoints();
                        break;
                    case "Polyline":
                        DrawInsulationOnPolyline();
                        break;
                    case "Block":
                        ImportInsulationBlock();
                        break;
                    default:
                        DrawInsulationOnTwoPointsTwoHeights(ppr);
                        break;
                }
            }
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        // 2 Points, 2 Heights
        private void DrawInsulationOnTwoPointsTwoHeights(PromptPointResult firstPoint)
        {
            #region input data

            var firstPointResult = firstPoint;
            var secondPointResult = _editorHelper.PromptForPoint("\nSelect SECOND point.", true, true,
                                                                 firstPointResult.Value);
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var thicknessAtFirstPointResult = _editorHelper.PromptForDouble("\nThickness at FIRST point : ",
                                                                            Settings.Default
                                                                                    .SInsulationThicknessAtFirstPoint);
            if (thicknessAtFirstPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            Settings.Default.SInsulationThicknessAtFirstPoint = thicknessAtFirstPointResult.Value;

            var thicknessAtSecondPointResult = _editorHelper.PromptForDouble("\nThickness at SECOND point : ",
                                                                             Settings.Default
                                                                                     .SInsulationThicknessAtSecondPoint);
            if (thicknessAtSecondPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            Settings.Default.SInsulationThicknessAtSecondPoint = thicknessAtSecondPointResult.Value;
            Settings.Default.Save();

            if (Math.Abs(firstPointResult.Value.Z) > 0.00001 || Math.Abs(secondPointResult.Value.Z) > 0.00001)
            {
                _editorHelper.WriteMessage("Soft Insulation should be placed in OXY plane !");
                _logger.Info("User input is invalid.");
                return;
            }
            #endregion

            #region preparation

            var minThickness = (thicknessAtFirstPointResult.Value < thicknessAtSecondPointResult.Value)
                                   ? thicknessAtFirstPointResult.Value
                                   : thicknessAtSecondPointResult.Value;
            var segmentsCount =
                (int)Math.Ceiling((firstPointResult.Value.DistanceTo(secondPointResult.Value) / minThickness) / 0.35);

            var botLeftPoint = new Complex(0, 0);
            var botRightPoint = new Complex(firstPointResult.Value.DistanceTo(secondPointResult.Value), 0);
            Complex topLeftPoint, topRightPoint;
            if (thicknessAtFirstPointResult.Value <= thicknessAtSecondPointResult.Value)
            {
                topLeftPoint = new Complex(0, thicknessAtFirstPointResult.Value);
                topRightPoint = new Complex(firstPointResult.Value.DistanceTo(secondPointResult.Value),
                                            thicknessAtSecondPointResult.Value);
            }
            else
            {
                topLeftPoint = new Complex(0, thicknessAtSecondPointResult.Value);
                topRightPoint = new Complex(firstPointResult.Value.DistanceTo(secondPointResult.Value),
                                            thicknessAtFirstPointResult.Value);
            }

            var vectorOfBaseLine = new Complex(secondPointResult.Value.X, secondPointResult.Value.Y) -
                                   new Complex(firstPointResult.Value.X, firstPointResult.Value.Y);

            var radiusBotCircle = firstPointResult.Value.DistanceTo(secondPointResult.Value) / (segmentsCount + 0.5);
            radiusBotCircle /= 2.0;

            var vectorTopEdge = topRightPoint - topLeftPoint;
            var vectorTopEdgeOrt = vectorTopEdge / vectorTopEdge.abs();
            var vectorNormalOfTopEdge = vectorTopEdgeOrt * (new Complex(0, 1.0));

            #endregion

            #region calculateCenterCircle

            var centersBotCircles = new List<Complex>();
            for (var i = 0; i < segmentsCount + 1; i++)
            {
                centersBotCircles.Add(new Complex(i * 2 * radiusBotCircle, radiusBotCircle));
            }

            var centersTopCircles = new List<KeyValuePair<double, Complex>>();
            var ordinate = new Line2d(new Complex(radiusBotCircle, 0), new Complex(radiusBotCircle, 100));
            var offsetTopEdge = new Line2d(topLeftPoint - vectorNormalOfTopEdge * radiusBotCircle,
                                           topRightPoint - vectorNormalOfTopEdge * radiusBotCircle);
            var centerFirstUpperCircle = offsetTopEdge.IntersectWitch(ordinate);
            centersTopCircles.Add(new KeyValuePair<double, Complex>(radiusBotCircle, centerFirstUpperCircle));

            if (((topRightPoint - topLeftPoint).arg()) * 180.0 / Math.PI <= 5)
            {
                for (var i = 1; i < segmentsCount + 1; i++)
                {
                    var rightVerticalEdge = new Line2d(botRightPoint, topRightPoint);
                    var lineParallelToX = new Line2d(centersTopCircles[i - 1].Value,
                                                     centersTopCircles[i - 1].Value +
                                                     new Complex(rightVerticalEdge.A, rightVerticalEdge.B));
                    var distFromCurrentCircleToRightVertEdge =
                        Math.Abs(
                            (centersTopCircles[i - 1].Value - lineParallelToX.IntersectWitch(rightVerticalEdge)).abs() -
                            centersTopCircles[i - 1].Key);
                    var radiusTopCircle = (distFromCurrentCircleToRightVertEdge / (segmentsCount - i + 0.5)) / 2.0;

                    var rR = centersTopCircles[i - 1].Key + radiusTopCircle;
                    var dR = Math.Abs(radiusTopCircle - centersTopCircles[i - 1].Key);
                    var distBetweenCenters = Math.Sqrt(rR * rR - dR * dR);
                    var centerNextTopCircle = centersTopCircles[i - 1].Value - vectorNormalOfTopEdge * dR +
                                              vectorTopEdgeOrt * distBetweenCenters;
                    centersTopCircles.Add(new KeyValuePair<double, Complex>(radiusTopCircle, centerNextTopCircle));
                }
            }
            else
            {

                for (var i = 1; i < segmentsCount + 1; i++)
                {
                    var tangentPoints = GetTangentPointsOfCommonExternalTanget(centersBotCircles[i], radiusBotCircle,
                                                                               centersTopCircles[i - 1].Value,
                                                                               centersTopCircles[i - 1].Key, true);
                    var vectorCommonTangent = tangentPoints.Value - tangentPoints.Key;
                    var vectorCommonTangentOrt = vectorCommonTangent / vectorCommonTangent.abs();
                    var vectorTranslationTopCircle = vectorCommonTangentOrt * (new Complex(0, 1.0));

                    var dL = new Line2d(tangentPoints.Value, tangentPoints.Value + vectorTranslationTopCircle * 100);
                    var rightVerticalEdge = new Line2d(botRightPoint, topRightPoint);
                    var dist = (tangentPoints.Value - dL.IntersectWitch(rightVerticalEdge)).abs();
                    var radiusTopCircle = (dist / (segmentsCount - i + 0.5)) / 2.0;

                    var hlpLine = new Line2d(tangentPoints.Key - vectorTranslationTopCircle * radiusTopCircle,
                                             tangentPoints.Value - vectorTranslationTopCircle * radiusTopCircle);
                    var offsetTopEdgeOnRadiusTopCircle = new Line2d(
                        topLeftPoint - vectorNormalOfTopEdge * radiusTopCircle,
                        topRightPoint - vectorNormalOfTopEdge * radiusTopCircle);
                    centersTopCircles.Add(new KeyValuePair<double, Complex>(radiusTopCircle,
                                                                            offsetTopEdgeOnRadiusTopCircle
                                                                                .IntersectWitch(hlpLine)));
                }
            }

            #endregion

            #region calculatePointsForPolylineInsulation
            var insulationPolyline = new Polyline();
            insulationPolyline.SetDatabaseDefaults();
            insulationPolyline.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
            var firstPairTangentPoints = GetTangentPointsOfCommonInternalTanget(centersBotCircles[0],
                                                                                radiusBotCircle,
                                                                                centersTopCircles[0].Value,
                                                                                centersTopCircles[0].Key, false);
            var ang = ((firstPairTangentPoints.Key - centersBotCircles[0]) / -centersBotCircles[0]).arg();
            insulationPolyline.AddVertexAt(0,
                                           new Point2d(firstPairTangentPoints.Key.real(),
                                                       firstPairTangentPoints.Key.imag()), -Math.Tan(ang / 4), 0, 0);
            insulationPolyline.AddVertexAt(0,
                                           new Point2d(firstPairTangentPoints.Value.real(),
                                                       firstPairTangentPoints.Value.imag()), 0, 0, 0);
            var secondPairTangentPoints = GetTangentPointsOfCommonInternalTanget(centersBotCircles[1],
                                                                                 radiusBotCircle,
                                                                                 centersTopCircles[0].Value,
                                                                                 centersTopCircles[0].Key, true);
            ang =
                ((centersTopCircles[0].Value - firstPairTangentPoints.Value) /
                 (centersTopCircles[0].Value - secondPairTangentPoints.Value)).arg();
            insulationPolyline.AddVertexAt(0,
                                           new Point2d(secondPairTangentPoints.Value.real(),
                                                       secondPairTangentPoints.Value.imag()), Math.Tan(ang / 4), 0,
                                           0);
            insulationPolyline.AddVertexAt(0,
                                           new Point2d(secondPairTangentPoints.Key.real(),
                                                       secondPairTangentPoints.Key.imag()), 0, 0, 0);
            var oldPair = secondPairTangentPoints;
            for (var i = 1; i < segmentsCount; i++)
            {
                var pairBotRightTopLeft = GetTangentPointsOfCommonInternalTanget(centersBotCircles[i],
                                                                                 radiusBotCircle,
                                                                                 centersTopCircles[i].Value,
                                                                                 centersTopCircles[i].Key,
                                                                                 false);
                var pairTopRightBotLeft = GetTangentPointsOfCommonInternalTanget(centersBotCircles[i + 1],
                                                                                 radiusBotCircle,
                                                                                 centersTopCircles[i].Value,
                                                                                 centersTopCircles[i].Key,
                                                                                 true);
                ang = ((pairBotRightTopLeft.Key - centersBotCircles[i]) / (oldPair.Key - centersBotCircles[i])).arg();
                insulationPolyline.AddVertexAt(0,
                                               new Point2d(pairBotRightTopLeft.Key.real(),
                                                           pairBotRightTopLeft.Key.imag()),
                                               -Math.Tan(ang / 4), 0, 0);
                insulationPolyline.AddVertexAt(0,
                                               new Point2d(pairBotRightTopLeft.Value.real(),
                                                           pairBotRightTopLeft.Value.imag()), 0, 0, 0);

                ang =
                    ((centersTopCircles[i].Value - pairBotRightTopLeft.Value) /
                     (centersTopCircles[i].Value - pairTopRightBotLeft.Value)).arg();
                insulationPolyline.AddVertexAt(0,
                                               new Point2d(pairTopRightBotLeft.Value.real(),
                                                           pairTopRightBotLeft.Value.imag()),
                                               Math.Tan(ang / 4), 0, 0);
                insulationPolyline.AddVertexAt(0,
                                               new Point2d(pairTopRightBotLeft.Key.real(),
                                                           pairTopRightBotLeft.Key.imag()), 0, 0, 0);
                oldPair = pairTopRightBotLeft;
            }
            var lastPairTangentPoints = GetTangentPointsOfCommonInternalTanget(centersBotCircles[segmentsCount],
                                                                               radiusBotCircle,
                                                                               centersTopCircles[segmentsCount]
                                                                                   .Value,
                                                                               centersTopCircles[segmentsCount].Key,
                                                                               false);
            ang =
                ((lastPairTangentPoints.Key - centersBotCircles[segmentsCount]) /
                 (oldPair.Key - centersBotCircles[segmentsCount])).arg();
            insulationPolyline.AddVertexAt(0,
                                           new Point2d(lastPairTangentPoints.Key.real(),
                                                       lastPairTangentPoints.Key.imag()), -Math.Tan(ang / 4), 0, 0);
            insulationPolyline.AddVertexAt(0,
                                           new Point2d(lastPairTangentPoints.Value.real(),
                                                       lastPairTangentPoints.Value.imag()), 0, 0, 0);

            var hlpPointForLastBulge = new Complex(centersTopCircles[segmentsCount].Value.real(),
                                                   centersTopCircles[segmentsCount].Value.imag() +
                                                   centersTopCircles[segmentsCount].Key);
            ang =
                ((centersTopCircles[segmentsCount].Value - lastPairTangentPoints.Value) /
                 (centersTopCircles[segmentsCount].Value - hlpPointForLastBulge)).arg();
            insulationPolyline.AddVertexAt(0, new Point2d(hlpPointForLastBulge.real(), hlpPointForLastBulge.imag()),
                                           Math.Tan(ang / 4), 0, 0);

            #endregion

            #region add contur to poly

            insulationPolyline.AddVertexAt(0, new Point2d(topRightPoint.real(), hlpPointForLastBulge.imag()), 0, 0,
                                           0);
            insulationPolyline.AddVertexAt(0, new Point2d(botRightPoint.real(), botRightPoint.imag()), 0, 0, 0);
            insulationPolyline.AddVertexAt(0, new Point2d(botLeftPoint.real(), botLeftPoint.imag()), 0, 0, 0);
            insulationPolyline.AddVertexAt(0, new Point2d(topLeftPoint.real(), topLeftPoint.imag()), 0, 0, 0);
            insulationPolyline.AddVertexAt(0, new Point2d(topRightPoint.real(), topRightPoint.imag()), 0, 0, 0);
            insulationPolyline.AddVertexAt(0, new Point2d(topRightPoint.real(), hlpPointForLastBulge.imag()), 0, 0,
                                           0);

            #endregion

            #region transforms

            if (thicknessAtFirstPointResult.Value > thicknessAtSecondPointResult.Value)
            {
                var acPtFrom = new Point3d(botRightPoint.x / 2, botRightPoint.y / 2, 0);
                var acPtTo = new Point3d(botRightPoint.x / 2, 100, 0);
                var acLine3d = new Line3d(acPtFrom, acPtTo);
                insulationPolyline.TransformBy(Matrix3d.Mirroring(acLine3d));
            }
            insulationPolyline.TransformBy(Matrix3d.Rotation(vectorOfBaseLine.arg(), new Vector3d(0, 0, 1),
                                                             Point3d.Origin));
            insulationPolyline.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(firstPointResult.Value)));
            insulationPolyline.TransformBy(_editorHelper.CurrentUcs);

            #endregion

            var oldLayer =
                    Application.DocumentManager.MdiActiveDocument.Editor.Document
                            .Database.Clayer;
            _documentHelper.LayerManipulator.CreateLayer("3-0", System.Drawing.Color.Lime);

            var softInsulationBlock = new BlockTableRecord();
            var nameSalt = DateTime.Now.GetHashCode().ToString();
            softInsulationBlock.Name = "SoftInsulation_" + nameSalt;
            softInsulationBlock.Origin = firstPointResult.Value;

            insulationPolyline.Layer = "0";

            #region addObjectsToDataBase
            using (var acTrans = _documentHelper.TransactionManager.StartTransaction())
            {
                var blockTable =
                    acTrans.GetObject(
                        Application.DocumentManager.MdiActiveDocument.Editor
                                .Document.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;

                blockTable.Add(softInsulationBlock);
                acTrans.AddNewlyCreatedDBObject(softInsulationBlock, true);

                softInsulationBlock.AppendEntity(insulationPolyline);
                acTrans.AddNewlyCreatedDBObject(insulationPolyline, true);

                var rigidInsulationRef = new BlockReference(firstPointResult.Value,
                                                            softInsulationBlock.ObjectId)
                { Layer = "3-0" };
                var currentSpace =
                    (BlockTableRecord)
                    acTrans.GetObject(
                        Application.DocumentManager.MdiActiveDocument.Editor
                                .Document.Database.CurrentSpaceId, OpenMode.ForWrite);

                currentSpace.AppendEntity(rigidInsulationRef);
                acTrans.AddNewlyCreatedDBObject(rigidInsulationRef, true);
                acTrans.Commit();
            }
            #endregion

            _documentHelper.LayerManipulator.ChangeLayer(oldLayer);
        }

        //4 Points
        private void DrawInsulationOnFourPoints()
        {
            var inputPoints = new Point3d[4];

            var firstPointResult = _editorHelper.PromptForPoint("\nSelect FIRST point of QUADRANGLE.");
            if (firstPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            inputPoints[0] = firstPointResult.Value;

            var secondPointResult = _editorHelper.PromptForPoint("\nSelect SECOND point of QUADRANGLE.");
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            inputPoints[1] = secondPointResult.Value;


            var thirdPointResult = _editorHelper.PromptForPoint("\nSelect THIRD point of QUADRANGLE.");
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            inputPoints[2] = thirdPointResult.Value;

            var fourthPointResult = _editorHelper.PromptForPoint("\nSelect FOURTH point of QUADRANGLE.");
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            inputPoints[3] = fourthPointResult.Value;

            CreateInsulationPolylineAndAppendToModelSpace(inputPoints, 5, true);
        }

        //3 Points
        private void DrawInsulationOnThreePoints()
        {
            var inputPoints = new Point3d[4];

            var firstPointResult = _editorHelper.PromptForPoint("\nSelect FIRST point of TRIANGLE.");
            if (firstPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            inputPoints[0] = firstPointResult.Value;

            var secondPointResult = _editorHelper.PromptForPoint("\nSelect SECOND point of TRIANGLE.");
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            inputPoints[1] = secondPointResult.Value;



            var thirdPointResult = _editorHelper.PromptForPoint("\nSelect THIRD point of TRIANGLE.");
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            inputPoints[3] = thirdPointResult.Value;
            inputPoints[2] = new Point3d((inputPoints[3].X + inputPoints[1].X) / 2.0,
                                         (inputPoints[3].Y + inputPoints[1].Y) / 2.0,
                                         (inputPoints[3].Z + inputPoints[1].Z) / 2.0);

            CreateInsulationPolylineAndAppendToModelSpace(inputPoints, 3, true);
        }

        //Polyline
        private void DrawInsulationOnPolyline()
        {
            var typedValues = new TypedValue[4];
            typedValues.SetValue(new TypedValue((int)DxfCode.Operator, "<or"), 0);
            typedValues.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE"), 1);
            typedValues.SetValue(new TypedValue((int)DxfCode.Start, "LWPOLYLINE"), 2);
            typedValues.SetValue(new TypedValue((int)DxfCode.Operator, "or>"), 3);

            var selectionFilter = new SelectionFilter(typedValues);
            var sOptions = new PromptSelectionOptions
            {
                MessageForAdding = "\r\nSelect polyline ",
                AllowDuplicates = true,
                SingleOnly = true
            };
            var selectionPromptResult = _editorHelper.PromptForSelection(sOptions, selectionFilter);
            if (selectionPromptResult.Status != PromptStatus.OK)
            {
                return;
            }

            if (selectionPromptResult.Value.Count > 1)
            {
                _editorHelper.WriteMessage("Select one polyline !");
                _logger.Info("User input is invalid.");
                return;
            }

            var unlinearSegment = false;
            var isClosedPolyline = true;
            var plID = selectionPromptResult.Value[0].ObjectId;
            var inputPointsFromPolyline = new Point3dCollection(); //save input points in array for easy sorting

            using (var acTrans = _documentHelper.TransactionManager.StartTransaction())
            {
                var polyline = acTrans.GetObject(plID, OpenMode.ForRead) as Polyline;
                if (!polyline.Closed)
                {
                    isClosedPolyline = false;
                }
                var numberOfVertices = polyline.NumberOfVertices;
                for (var i = 0; i < numberOfVertices; i++)
                {
                    var currentPoint = polyline.GetPoint3dAt(i);
                    var exist = false;
                    if (polyline.GetBulgeAt(i) > 0.0)
                    {
                        unlinearSegment = true;
                        break;
                    }
                    foreach (Point3d point in inputPointsFromPolyline) //check for exist
                    {
                        if (point.DistanceTo(currentPoint) < 0.00000254)
                            exist = true;
                    }
                    if (!exist)
                    {
                        inputPointsFromPolyline.Add(currentPoint);
                    }
                }
            }

            if (!isClosedPolyline)
            {
                _editorHelper.WriteMessage("Polyline is NOT closed !");
                _logger.Info("User input is invalid.");
                return;
            }

            if (unlinearSegment)
            {
                _editorHelper.WriteMessage("Polyline contains the arc Segment !");
                _logger.Info("User input is invalid.");
                return;
            }

            if ((inputPointsFromPolyline.Count > 4) || (inputPointsFromPolyline.Count < 3))
            {
                _editorHelper.WriteMessage("Polyline has more than four Points or less than three !");
                _logger.Info("User input is invalid.");
                return;
            }

            switch (inputPointsFromPolyline.Count)
            {
                case 3:
                    var inputPointsTriangle = new Point3d[4];
                    inputPointsTriangle[0] = inputPointsFromPolyline[0];
                    inputPointsTriangle[1] = inputPointsFromPolyline[1];
                    inputPointsTriangle[3] = inputPointsFromPolyline[2];
                    inputPointsTriangle[2] = new Point3d((inputPointsTriangle[3].X + inputPointsTriangle[1].X) / 2.0,
                                                         (inputPointsTriangle[3].Y + inputPointsTriangle[1].Y) / 2.0,
                                                         (inputPointsTriangle[3].Z + inputPointsTriangle[1].Z) / 2.0);
                    CreateInsulationPolylineAndAppendToModelSpace(inputPointsTriangle, 3, false);
                    break;
                case 4:
                    var inputPointsQuadrangle = new Point3d[4];
                    for (var i = 0; i < 4; i++)
                    {
                        inputPointsQuadrangle[i] = inputPointsFromPolyline[i];
                    }
                    CreateInsulationPolylineAndAppendToModelSpace(inputPointsQuadrangle, 5, false);
                    break;
            }

        }

        //Dynamic block
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
                _editorHelper.WriteMessage("Dynamic block SoftInsulation.dwg does not exist.");
                return;
            }
            _documentHelper.ImportDynamicBlockAndFillItsProperties(
               dynamicBlockPath, insertionPointResult.Value, new System.Collections.Hashtable() { { "Distance1", 10d } }, new System.Collections.Hashtable());
        }

        private void CreateInsulationPolylineAndAppendToModelSpace(Point3d[] inputPoints, int rk,
                                                                   bool drawPerimeterOfInsulation)
        {
            var inputQPoints = new Quaternion[4]; // completed after sorting

            #region sorting

            var longestDistance = 0.0;
            var indexFirstPoint = 0;
            for (var i = 1; i < 4; i++)
            {
                var currentDistance = inputPoints[i - 1].DistanceTo(inputPoints[i]);
                if (currentDistance > longestDistance)
                {
                    longestDistance = currentDistance;
                    indexFirstPoint = i - 1;
                }
            }
            if (inputPoints[3].DistanceTo(inputPoints[0]) > longestDistance)
            {
                indexFirstPoint = 3;
            }

            var tempPoints = new Point3d[4];
            for (var i = indexFirstPoint; i < 4 + indexFirstPoint; i++)
            {
                if (i < 4)
                    tempPoints[i - indexFirstPoint] = inputPoints[i];
                else
                    tempPoints[i - indexFirstPoint] = inputPoints[i - 4];
            }

            for (var i = 0; i < 4; i++)
            {
                inputPoints[i] = tempPoints[i];
                inputQPoints[i] = new Quaternion(0.0, inputPoints[i].X, inputPoints[i].Y, inputPoints[i].Z);
            }

            #endregion

            var uCSOfInputPoints = new UCS(inputQPoints[0], inputQPoints[1], inputQPoints[2]);

            if (Math.Abs((uCSOfInputPoints.FromACS(inputQPoints[3])).GetZ()) > 0.00000254)
            {
                _editorHelper.WriteMessage("Four Points are not in the same Plane !");
                _logger.Info("User input is invalid.");
                return;
            }

            #region counterclockwise selection test

            var centerUcsOfInputPoints = new Quaternion(0, 0, 0, 0);
            var zAxisInUscOfInputPoints = new Quaternion(0, 0, 0, 1000);
            var zAxisInAcs = uCSOfInputPoints.ToACS(zAxisInUscOfInputPoints) -
                             uCSOfInputPoints.ToACS(centerUcsOfInputPoints);
            if (zAxisInAcs.GetZ() < 0)
            {
                var tempPoint = new Point3d();
                var tempQuaternion = new Quaternion();

                tempPoint = inputPoints[0];
                inputPoints[0] = inputPoints[1];
                inputPoints[1] = tempPoint;

                tempQuaternion = inputQPoints[0];
                inputQPoints[0] = inputQPoints[1];
                inputQPoints[1] = tempQuaternion;

                tempPoint = inputPoints[2];
                inputPoints[2] = inputPoints[3];
                inputPoints[3] = tempPoint;

                tempQuaternion = inputQPoints[2];
                inputQPoints[2] = inputQPoints[3];
                inputQPoints[3] = tempQuaternion;

                uCSOfInputPoints = new UCS(inputQPoints[0], inputQPoints[1], inputQPoints[2]);
            }

            #endregion

            var ucsPoints = new Point3d[4];
            //are coordinates in a 0.0 WCS to draw (drawing only in oXY 0,0 per WCS and transform when it is ready polyline transform)
            var ucsQPoints = new Quaternion[4];
            //are coordinates in a 0.0 WCS to draw (drawing only in oXY 0,0 per WCS and when it is ready polyline transform)

            var complexPoints = new Complex[4];
            var sidesOffPolygon = new Complex[4];

            #region 2d prepare

            for (int i = 0; i < 4; i++)
            {
                Quaternion q = uCSOfInputPoints.FromACS(inputQPoints[i]);
                ucsQPoints[i] = new Quaternion(0, (Math.Abs(q.GetX()) > 0.00000254) ? q.GetX() : 0.0,
                                               (Math.Abs(q.GetY()) > 0.00000254) ? q.GetY() : 0.0,
                                               (Math.Abs(q.GetZ()) > 0.00000254) ? q.GetZ() : 0.0);
                ucsPoints[i] = new Point3d(ucsQPoints[i].GetX(), ucsQPoints[i].GetY(), ucsQPoints[i].GetZ());
                complexPoints[i] = new Complex(ucsQPoints[i].GetX(), ucsQPoints[i].GetY());
                if (i > 0)
                {
                    sidesOffPolygon[i - 1] = complexPoints[i] - complexPoints[i - 1];
                }
            }
            sidesOffPolygon[3] = complexPoints[0] - complexPoints[3];

            #endregion

            #region convex check

            for (var i = 0; i < 4; i++)
            {
                if (Math.Abs(GetAngleInPointIndex(i, complexPoints)) >= Math.PI)
                {
                    break;
                }
            }

            #endregion

            var minThickness = (complexPoints[2].imag() < complexPoints[3].imag())
                                   ? complexPoints[2].imag()
                                   : complexPoints[3].imag();
            var segmentsCount = (int)Math.Ceiling((sidesOffPolygon[0].abs() / minThickness) / 0.35);

            var radius = sidesOffPolygon[0].abs() * 2 / (segmentsCount * 2);
            radius /= rk;

            var lowerLine = new List<Complex>();
            var upperLine = new List<Complex>();


            var insulationPerimeter = new Polyline();
            insulationPerimeter.SetDatabaseDefaults();
            insulationPerimeter.AddVertexAt(0, new Point2d(complexPoints[0].real(), complexPoints[0].imag()), 0, 0,
                                            0);
            insulationPerimeter.AddVertexAt(1, new Point2d(complexPoints[1].real(), complexPoints[1].imag()), 0, 0,
                                            0);
            insulationPerimeter.AddVertexAt(2, new Point2d(complexPoints[2].real(), complexPoints[2].imag()), 0, 0,
                                            0);
            insulationPerimeter.AddVertexAt(3, new Point2d(complexPoints[3].real(), complexPoints[3].imag()), 0, 0,
                                            0);
            insulationPerimeter.AddVertexAt(4, new Point2d(complexPoints[0].real(), complexPoints[0].imag()), 0, 0,
                                            0);

            var objectsCollection = insulationPerimeter.GetOffsetCurves(-radius);
            var acPolyOffset = objectsCollection[0] as Polyline;

            var pointsFromOffset = new Complex[4];
            for (var i = 0; i < 4; i++)
            {
                var point = acPolyOffset.GetPoint3dAt(i);
                pointsFromOffset[i] = new Complex(point.X, point.Y);
            }

            var variant = 0;
            if (pointsFromOffset[3].real() < pointsFromOffset[0].real() &&
                (pointsFromOffset[3] - pointsFromOffset[0]).abs() / (radius * 2) > 1 &&
                Math.Abs(pointsFromOffset[2].real() - pointsFromOffset[1].real()) > radius * 2)
            {
                variant = 1;
                lowerLine.Add(pointsFromOffset[0]);
                var xPos = pointsFromOffset[0].real();
                do
                {
                    xPos += radius * 2.0;
                    if (xPos <= pointsFromOffset[1].real() + radius * 2 / 3)
                    {
                        lowerLine.Add(new Complex(xPos, radius));
                    }
                } while (xPos < (pointsFromOffset[0] - pointsFromOffset[1]).abs() + radius * 2 / 3);

                xPos = 0;
                var ort = (pointsFromOffset[3] - pointsFromOffset[0]) /
                          (pointsFromOffset[3] - pointsFromOffset[0]).abs();
                var k = Math.Abs(Math.Cos(ort.arg()));
                k -= 0.01;
                do
                {
                    xPos += 2 * radius / Math.Abs(k);
                    if (xPos < (pointsFromOffset[3] - pointsFromOffset[0]).abs() + radius * 2 / 3)
                    {
                        lowerLine.Insert(0, ort * xPos + pointsFromOffset[0]);
                    }
                } while (xPos < (pointsFromOffset[3] - pointsFromOffset[0]).abs() + radius * 2 / 3);
                lowerLine.RemoveAt(0);
            }

            if (pointsFromOffset[1].real() < pointsFromOffset[2].real() &&
                (pointsFromOffset[2] - pointsFromOffset[1]).abs() / (radius * 2) > 1 &&
                Math.Abs(pointsFromOffset[2].real() - pointsFromOffset[1].real()) > radius * 2)
            {
                variant = 2;
                lowerLine.Add(pointsFromOffset[1]);
                var xPos = pointsFromOffset[1].real();
                do
                {
                    xPos -= radius * 2.0;
                    if (xPos >= pointsFromOffset[0].real() - radius * 2.0 / 3.0)
                    {
                        lowerLine.Insert(0, new Complex(xPos, radius));
                    }
                } while (xPos > pointsFromOffset[0].real() - radius * 2.0 / 3.0);


                xPos = 0;
                var ort = (pointsFromOffset[2] - pointsFromOffset[1]) /
                          (pointsFromOffset[2] - pointsFromOffset[1]).abs();
                var k = Math.Abs(Math.Cos(ort.arg()));
                k -= 0.01;
                do
                {
                    xPos += 2 * radius / Math.Abs(k);
                    if (xPos < (pointsFromOffset[2] - pointsFromOffset[1]).abs() + radius * 2.0 / 3.0)
                    {
                        lowerLine.Add(ort * xPos + pointsFromOffset[1]);
                    }
                } while (xPos < (pointsFromOffset[2] - pointsFromOffset[1]).abs() + radius * 2.0 / 3.0);

            }
            if (variant == 0)
            {
                lowerLine.Add(pointsFromOffset[0]);
                var xPos = pointsFromOffset[0].real();
                do
                {
                    xPos += radius * 2.0;
                    if (xPos <= pointsFromOffset[1].real() + radius * 2.0)
                    {
                        lowerLine.Add(new Complex(xPos, radius));
                    }
                } while (xPos < (pointsFromOffset[0] - pointsFromOffset[1]).abs() + radius * 2.0);
            }

            var insulationPolyline = new Polyline();
            insulationPolyline.SetDatabaseDefaults();
            var old = new KeyValuePair<Complex, Complex>();
            for (var i = 1; i < lowerLine.Count; i++)
            {
                var hlpPoint = (lowerLine[i] + lowerLine[i - 1]) / 2.0;

                var verticalLine = new Line(new Point3d(hlpPoint.real(), hlpPoint.imag(), 0),
                                            new Point3d(hlpPoint.real(), hlpPoint.imag() + 0.01, 0));

                var pts = new Point3dCollection();
                try
                {
                    acPolyOffset.IntersectWith(verticalLine, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                }
                catch (Exception aCadRuntimeException)
                {
                    _logger.Error("Unable to Intersect 2 entities", aCadRuntimeException);
                }
                if (pts.Count != 2)
                {
                    continue;
                }
                var p = (pts[0].Y > pts[1].Y) ? pts[0] : pts[1];
                var cc = new Complex(p.X, p.Y);
                if ((cc - hlpPoint).abs() > 2 * radius)
                {
                    upperLine.Add(new Complex(p.X, p.Y));

                    var tangentPointsTopLeftBotRight = GetTangentPointsOfCommonInternalTanget(cc, radius,
                                                                                              lowerLine[i], radius,
                                                                                              true);
                    var tangentPointsBotRightTopLeft = GetTangentPointsOfCommonInternalTanget(lowerLine[i - 1],
                                                                                              radius, cc, radius,
                                                                                              false);

                    if (i > 1)
                    {
                        double ang1 = 0;

                        ang1 =
                            ((lowerLine[i - 1] - tangentPointsBotRightTopLeft.Key) / (lowerLine[i - 1] - old.Value))
                                .arg();
                        insulationPolyline.AddVertexAt(0,
                                                       new Point2d(tangentPointsBotRightTopLeft.Key.real(),
                                                                   tangentPointsBotRightTopLeft.Key.imag()),
                                                       -Math.Tan(ang1 / 4), 0, 0);

                    }
                    else
                    {
                        insulationPolyline.AddVertexAt(0,
                                                       new Point2d(tangentPointsBotRightTopLeft.Key.real(),
                                                                   tangentPointsBotRightTopLeft.Key.imag()), 0, 0, 0);
                    }
                    double ang =
                        ((cc - tangentPointsBotRightTopLeft.Value) / (cc - tangentPointsTopLeftBotRight.Key)).arg();
                    insulationPolyline.AddVertexAt(0,
                                                   new Point2d(tangentPointsBotRightTopLeft.Value.real(),
                                                               tangentPointsBotRightTopLeft.Value.imag()), 0, 0, 0);

                    insulationPolyline.AddVertexAt(0,
                                                   new Point2d(tangentPointsTopLeftBotRight.Key.real(),
                                                               tangentPointsTopLeftBotRight.Key.imag()),
                                                   Math.Tan(ang / 4), 0, 0);
                    insulationPolyline.AddVertexAt(0,
                                                   new Point2d(tangentPointsTopLeftBotRight.Value.real(),
                                                               tangentPointsTopLeftBotRight.Value.imag()), 0, 0, 0);

                    old = tangentPointsTopLeftBotRight;
                }
                else
                {

                    if (i < lowerLine.Count / 2)
                    {
                        lowerLine.RemoveAt(i - 1);
                    }
                    else
                    {
                        lowerLine.RemoveAt(i);
                    }
                    i--;
                }
            }

            var vectorBaseLine = new Complex(inputPoints[1].X, inputPoints[1].Y) -
                                 new Complex(inputPoints[0].X, inputPoints[0].Y);

            #region transforms

            insulationPolyline.TransformBy(Matrix3d.Rotation(vectorBaseLine.arg(), new Vector3d(0, 0, 1),
                                                             Point3d.Origin));
            insulationPerimeter.TransformBy(Matrix3d.Rotation(vectorBaseLine.arg(), new Vector3d(0, 0, 1),
                                                              Point3d.Origin));

            insulationPolyline.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(inputPoints[0])));
            insulationPerimeter.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(inputPoints[0])));

            insulationPolyline.TransformBy(_editorHelper.CurrentUcs);
            insulationPerimeter.TransformBy(_editorHelper.CurrentUcs);

            #endregion

            var oldLayer = _documentHelper.Database.Clayer;

            _documentHelper.LayerManipulator.CreateLayer("3-0", System.Drawing.Color.Lime);
            insulationPolyline.Layer = "0";
            var softInsulationBlock = new BlockTableRecord();
            var nameSalt = DateTime.Now.GetHashCode().ToString();
            softInsulationBlock.Name = "SoftInsulation_" + nameSalt;
            softInsulationBlock.Origin = inputPoints[0];

            using (var acTrans = _documentHelper.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(_documentHelper.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                acBlkTbl.Add(softInsulationBlock);
                acTrans.AddNewlyCreatedDBObject(softInsulationBlock, true);
                softInsulationBlock.AppendEntity(insulationPolyline);
                acTrans.AddNewlyCreatedDBObject(insulationPolyline, true);
                if (drawPerimeterOfInsulation)
                {
                    insulationPerimeter.Layer = "0";
                    softInsulationBlock.AppendEntity(insulationPerimeter);
                    acTrans.AddNewlyCreatedDBObject(insulationPerimeter, true);
                }
                var rigidInsulationRef = new BlockReference(inputPoints[0],
                                                            softInsulationBlock.ObjectId)
                { Layer = "3-0" };
                var currentSpace =
                    (BlockTableRecord)acTrans.GetObject(_documentHelper.Database.CurrentSpaceId, OpenMode.ForWrite);
                currentSpace.AppendEntity(rigidInsulationRef);
                acTrans.AddNewlyCreatedDBObject(rigidInsulationRef, true);
                acTrans.Commit();
            }
            _documentHelper.LayerManipulator.ChangeLayer(oldLayer);
        }

        private KeyValuePair<Complex, Complex> GetTangentPointsOfCommonInternalTanget(Complex centerFirstCircle,
                                                                                      double radiusFirstCircle,
                                                                                      Complex centerSecondCircle,
                                                                                      double radiusSecondCircle,
                                                                                      bool isRisingThrough)
        {

            var cathetus = radiusFirstCircle + radiusSecondCircle;
            var centralAngleFi = Math.Asin(cathetus / (centerSecondCircle - centerFirstCircle).abs());
            centralAngleFi = Math.PI / 2.0 - centralAngleFi;
            var angleBetweenOxAndCathetus = (centerSecondCircle - centerFirstCircle).arg() +
                                            ((isRisingThrough) ? centralAngleFi : -centralAngleFi);

            var endPointOfCathetus = centerFirstCircle + Complex.polar(cathetus, angleBetweenOxAndCathetus);
            var tangentPointAtFirstCircle = centerFirstCircle +
                                            Complex.polar(radiusFirstCircle, angleBetweenOxAndCathetus);

            var secondCathetus = centerSecondCircle - endPointOfCathetus;
            var tangentPointAtSecondCircle = tangentPointAtFirstCircle + secondCathetus;

            return new KeyValuePair<Complex, Complex>(tangentPointAtFirstCircle, tangentPointAtSecondCircle);
        }

        private KeyValuePair<Complex, Complex> GetTangentPointsOfCommonExternalTanget(Complex centerFirstCircle,
                                                                                      double radiusFirstCircle,
                                                                                      Complex centerSecondCircle,
                                                                                      double radiusSecondCircle, bool b)
        {
            var cathetus = radiusSecondCircle - radiusFirstCircle;
            var centralAngleFi = Math.Asin(cathetus / (centerSecondCircle - centerFirstCircle).abs());
            centralAngleFi = Math.PI / 2.0 - centralAngleFi;
            var angleBetweenOxAndCathetus = (centerSecondCircle - centerFirstCircle).arg() +
                                            ((b) ? centralAngleFi : -centralAngleFi);

            var Q = centerFirstCircle - Complex.polar(-cathetus, angleBetweenOxAndCathetus);
            var m = centerFirstCircle - Complex.polar(radiusFirstCircle, angleBetweenOxAndCathetus);

            var w = centerSecondCircle - Q;
            var n = m + w;

            return new KeyValuePair<Complex, Complex>(m, n);
        }

        private double GetAngleInPointIndex(int pointIndex, Complex[] arr)
        {
            var preIndex = pointIndex - 1;
            var nextIndex = pointIndex + 1;
            if (preIndex < 0)
            {
                preIndex = 3;
            }
            if (nextIndex > 3)
            {
                nextIndex = 0;
            }

            var vectorNextEdge = arr[nextIndex] - arr[pointIndex];
            var vectorPrevEdge = arr[preIndex] - arr[pointIndex];

            return (vectorPrevEdge / vectorNextEdge).arg();
        }
    }
}