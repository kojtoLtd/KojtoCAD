using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.Interfaces;
using System;
using System.Collections;
using System.Reflection;
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

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.Level))]

namespace KojtoCAD.BlockItems
{
    public class Level
    {
        private readonly Document _doc = Application.DocumentManager.MdiActiveDocument;
        private readonly Database _db = Application.DocumentManager.MdiActiveDocument.Database;
        private readonly ILogger _logger;
        private readonly IBlockDrawingProvider _blockDrawingProvider;
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;

        public Level()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            IoC.ContainerRegistrar.Container.Resolve<IPaperSizeFactory>();
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        ///  Level sign
        /// </summary>
        [CommandMethod("level")]
        public void LevelHeightSignStart()
        {
            var insertionPointResult = _editorHelper.PromptForPoint("Pick insertion point : ");
            if (insertionPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var dynamicBlockProperties = new Hashtable();
            var dynamicBlockAttributes = new Hashtable();
            dynamicBlockProperties.Add("Base Point", insertionPointResult.Value);
            var dynamicBlockPath = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockPath == null)
            {
                _editorHelper.WriteMessage("Dynamic block LevelHeightSign.dwg does not exist.");
                return;
            }
            var brefId = _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                dynamicBlockPath, insertionPointResult.Value, dynamicBlockProperties, dynamicBlockAttributes);

            using (var transaction = _doc.TransactionManager.StartTransaction())
            {
                var blockReference = (BlockReference)transaction.GetObject(brefId, OpenMode.ForWrite);

                var zAxis = _doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis;
                var xAxis = _doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Xaxis;
                var yAxis = _doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Yaxis;
                var origin = _doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Origin;

                var mat = Matrix3d.AlignCoordinateSystem(
                    Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, origin, xAxis, yAxis, zAxis);

                blockReference.TransformBy(mat);

                var ORIGIN = new Quaternion(0, origin.X, origin.Y, origin.Z);
                var axeX = new Quaternion(0, xAxis.X, xAxis.Y, xAxis.Z);
                var axeY = new Quaternion(0, yAxis.X, yAxis.Y, yAxis.Z);

                var ucs = new UCS(ORIGIN, ORIGIN + axeX, ORIGIN + axeY);
                var basePoint = new Quaternion(
                    0, insertionPointResult.Value.X, insertionPointResult.Value.Y, insertionPointResult.Value.Z);
                basePoint = ucs.ToACS(basePoint);
                ucs = new UCS(ORIGIN, ORIGIN + axeX, basePoint);

                var y = ucs.FromACS(basePoint).GetY();
                if (double.IsNaN(y))
                {
                    y = 0.0;
                }

                foreach (ObjectId attId in blockReference.AttributeCollection)
                {
                    var attributeReference = (AttributeReference)transaction.GetObject(attId, OpenMode.ForWrite);
                    if (attributeReference.Tag == "LEVEL")
                    {
                        var mess = y.ToString("0.#####");
                        attributeReference.TextString = (mess.Length > 0) ? mess : "0.0";
                    }
                }
                transaction.Commit();
            }
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Recalculate levels of level signs
        /// </summary>
        [CommandMethod("relevel")]
        public void RecalcLevelHeightSignStart()
        {
            var objectIdCollection = _editorHelper.PromptForSelection("Select LEVEL  Entites:");
            if (objectIdCollection.Status != PromptStatus.OK)
            {
                return;
            }

            var origin = _doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Origin;
            var ORIGIN = new Quaternion(0, origin.X, origin.Y, origin.Z);

            using (var transaction = _doc.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(_db.BlockTableId, OpenMode.ForRead);

                // TODO : Level must not be hard coded. TDD - convensions test
                var blockTableRecordLevelSign =
                    (BlockTableRecord)transaction.GetObject(blockTable["Level"], OpenMode.ForWrite);
                var btrIDs = new UtilityClass().GetAllRefernecesOfBtr(blockTableRecordLevelSign.ObjectId, transaction);
                foreach (ObjectId asObjId in btrIDs)
                {
                    var ok = false;
                    for (var i = 0; i < objectIdCollection.Value.GetObjectIds().Length; i++)
                    {
                        if (asObjId == objectIdCollection.Value.GetObjectIds()[i])
                        {
                            ok = true;
                            break;
                        }
                    }
                    if (ok == false)
                    {
                        continue;
                    }
                    var blockReference = transaction.GetObject(asObjId, OpenMode.ForWrite) as BlockReference;
                    var basePoint = blockReference.Position;
                    var normal = blockReference.Normal;
                    var xAxis = new Point3d();
                    var testP = new Point3d();

                    var dbObjectCollection = new DBObjectCollection();
                    blockReference.Explode(dbObjectCollection);
                    foreach (Entity entity in dbObjectCollection)
                    {
                        if (entity.ColorIndex == 1)
                        {
                            var line = entity as Line;
                            xAxis = line.StartPoint;

                            if (xAxis.DistanceTo(basePoint) < 0.5)
                            {
                                xAxis = line.EndPoint;
                            }

                        }
                        else
                        {
                            var str = entity.GetType().ToString();
                            if (str.Contains("Poly"))
                            {
                                var polyline = entity as Polyline;
                                testP = polyline.GetLineSegmentAt(1).MidPoint;
                            }
                        }
                    }
                    var op = new Quaternion(0, basePoint.X, basePoint.Y, basePoint.Z);
                    var ox = new Quaternion(0, xAxis.X, xAxis.Y, xAxis.Z);
                    var oz = new Quaternion(0, normal.X, normal.Y, normal.Z);
                    var testQ = new Quaternion(0, testP.X, testP.Y, testP.Z);

                    var ucs = new UCS(op, ox, op + oz);
                    if (ucs.FromACS(testQ).GetZ() < 0)
                    {
                        ucs = new UCS(op, op + oz, ox);
                    }

                    var y = -ucs.FromACS(ORIGIN).GetZ();

                    foreach (ObjectId attId in blockReference.AttributeCollection)
                    {
                        var attributeReference = (AttributeReference)transaction.GetObject(attId, OpenMode.ForWrite);
                        if (attributeReference.Tag != "LEVEL")
                        {
                            continue;
                        }
                        var oldStr = "\nold = " + attributeReference.TextString;
                        if (double.IsNaN(y))
                        {
                            y = 0.0;
                        }
                        var mess = y.ToString("0.#####");
                        attributeReference.TextString = ((mess.Length > 0) ? mess : "0.0");
                        _doc.Editor.WriteMessage(oldStr + " / " + DateTime.Now.ToString("d"));
                    }
                }
                transaction.Commit();
            }
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }
    }
}
