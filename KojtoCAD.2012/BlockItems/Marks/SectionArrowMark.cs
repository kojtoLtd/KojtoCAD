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
[assembly: CommandClass(typeof(KojtoCAD.BlockItems.Marks.SectionArrowMark))]

namespace KojtoCAD.BlockItems.Marks
{
    public class SectionArrowMark
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public SectionArrowMark()
        {
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Section Arrow Mark
        /// </summary>
        [CommandMethod("sm")]
        public void SectionArrowMarkStart()
        {
            Matrix3d currentUcs = _editorHelper.CurrentUcs;
            Vector3d xAxis = currentUcs.CoordinateSystem3d.Xaxis;
            Vector3d yAxis = currentUcs.CoordinateSystem3d.Yaxis;
            xAxis = (xAxis + yAxis) / 2.0;

            var basePointResult = _editorHelper.PromptForPoint("Pick arrow point : ");
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

            var P3 = new Quaternion(0, basePoint.X + xAxis.X, basePoint.Y + xAxis.Y, basePoint.Z + xAxis.Z);
            var p3 = new Point3d(P3.GetX(), P3.GetY(), P3.GetZ());

            var newUcs = GeometryUtility.GetUcs(basePoint, endPoint, p3, true);

            double dist = basePoint.DistanceTo(endPoint);

            basePoint = new Point3d(0, 0, 0);
            endPoint = new Point3d(dist, 0, 0);

            Hashtable dynamicBlockProperties = new Hashtable();
            Hashtable dynamicBlockAttributes = new Hashtable();
            dynamicBlockProperties.Add("Base Point", basePoint);
            dynamicBlockProperties.Add("Ref Line Length", dist - 7.9443);
            double angle = GeometryUtility.GetAngleFromXAxis(basePoint, endPoint) - Math.PI / 2;
            dynamicBlockProperties.Add("Symbol Rotation", angle);
            dynamicBlockProperties.Add("Arrow Direction", (Int16)1);


            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block SectionArrowMark.dwg does not exist.");
                return;
            }
            ObjectId refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                dynamicBlockFullName,
                basePoint,
                dynamicBlockProperties,
                dynamicBlockAttributes);


            using (var acTrans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                var entity = (Entity)acTrans.GetObject(refO, OpenMode.ForWrite);
                entity.TransformBy(newUcs);
                acTrans.Commit();
            }

            _editorHelper.CurrentUcs = currentUcs;

            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }
    }
}
