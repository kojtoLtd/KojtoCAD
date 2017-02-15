using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
using System;
using System.Collections;
using System.Reflection;
using KojtoCAD.Mathematics.Geometry;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.Marks.SectionArrowPairMark))]

namespace KojtoCAD.BlockItems.Marks
{
    public class SectionArrowPairMark
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public SectionArrowPairMark()
        {
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Section Arrow Pair Mark
        /// </summary>
        [CommandMethod("sp")]
        public void SectionArrowPairMarkStart()
        {
            Matrix3d ucs = _editorHelper.CurrentUcs;
            Vector3d xAxis = ucs.CoordinateSystem3d.Xaxis;
            Vector3d yAxis = ucs.CoordinateSystem3d.Yaxis;
            xAxis = (xAxis + yAxis) / 2.0;

            var basePointResult = _editorHelper.PromptForPoint("Pick base point : ");
            if (basePointResult.Status != PromptStatus.OK)
            {
                return;
            }
            var basePoint = basePointResult.Value.TransformBy(GeometryUtility.GetTransforMatrixToWcs());

            var endPointResult = _editorHelper.PromptForPoint("Pick end point : ", true, true, basePoint);
            if (endPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            var endPoint = endPointResult.Value.TransformBy(GeometryUtility.GetTransforMatrixToWcs());

            if (basePoint == endPoint)
            {
                _editorHelper.WriteMessage("\nBase point and end point can not be the same or be less then 8 units closer. Try again and place the end point further from the base point.");
                return;
            }

            _editorHelper.CurrentUcs = Matrix3d.Identity;

            Quaternion p3Quaternion = new Quaternion(0, basePoint.X + xAxis.X, basePoint.Y + xAxis.Y, basePoint.Z + xAxis.Z);
            Point3d p3 = new Point3d(p3Quaternion.GetX(), p3Quaternion.GetY(), p3Quaternion.GetZ());

            Matrix3d newUcs = GeometryUtility.GetUcs(basePoint, endPoint, p3, true);

            double dist = basePoint.DistanceTo(endPoint);

            basePoint = new Point3d(0, 0, 0);
            endPoint = new Point3d(dist, 0, 0);


            Hashtable dynamicBlockProperties = new Hashtable();
            Hashtable dynamicBlockAttributes = new Hashtable();
            dynamicBlockProperties.Add("Base Point", basePoint);
            double lineLength = endPoint.DistanceTo(basePoint) - 20;
            dynamicBlockProperties.Add("Ref Line Length", lineLength < 0 ? 10.0 : lineLength);  // 10 because it crashed if it is minus

            double angle = GeometryUtility.GetAngleFromXAxis(basePoint, endPoint) - Math.PI / 2;
            dynamicBlockProperties.Add("Symbol Rotation", angle);
            dynamicBlockProperties.Add("Arrow Direction", (Int16)1);


            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block SectionArrowPairMark.dwg does not exist.");
                return;
            }
            ObjectId refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                dynamicBlockFullName,
                basePoint,
                dynamicBlockProperties,
                dynamicBlockAttributes);

            using (Transaction transaction = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                var entity = (Entity)transaction.GetObject(refO, OpenMode.ForWrite);
                entity.TransformBy(newUcs);
                transaction.Commit();
            }
            _editorHelper.CurrentUcs = ucs;
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }
    }
}
