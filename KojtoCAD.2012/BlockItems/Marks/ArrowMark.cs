using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
using System;
using System.Collections;
using System.Reflection;
using KojtoCAD.Mathematics.Geometry;
using Exception = System.Exception;
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

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.Marks.ArrowMark))]

namespace KojtoCAD.BlockItems.Marks
{
    

    public class ArrowMark
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper; 
        private static ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public ArrowMark()
        {
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Arrow Mark
        /// </summary>
        [CommandMethod("am")]
        public void ArrowMarkStart()
        {
            var basePointResult = _editorHelper.PromptForPoint("Pick arrow point : ");
            if (basePointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var directionPointResult = _editorHelper.PromptForPoint("Pick direction point : ", true, true, basePointResult.Value);
            if (directionPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            if (directionPointResult.Value == basePointResult.Value)
            {
                _editorHelper.WriteMessage("\nBase point and direction point can not be the same or be less then 8 units closer. Try again and place the direction point further from the base point.");
                return;
            }

            Vector3d xAxis = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Xaxis;
            Vector3d yAxis = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Yaxis;
            xAxis = (xAxis + yAxis) / 2.0;

            Matrix3d ucs = _editorHelper.CurrentUcs;

            basePointResult.Value.TransformBy(GeometryUtility.GetTransforMatrixToWcs());
            directionPointResult.Value.TransformBy(GeometryUtility.GetTransforMatrixToWcs());

            

            _editorHelper.CurrentUcs = Matrix3d.Identity;
            Point3d endPoint = directionPointResult.Value;
            Vector3d vx = new Vector3d(endPoint.X - basePointResult.Value.X, endPoint.Y - basePointResult.Value.Y, endPoint.Z - basePointResult.Value.Z);
            Quaternion qx = new Quaternion(0, vx.X, vx.Y, vx.Z);
            Quaternion qy = new Quaternion(0, xAxis.X, xAxis.Y, xAxis.Z);
            UCS UCS = new UCS(new Quaternion(0, 0, 0, 0), qx, qy);
            qx = UCS.ToACS(new Quaternion(0, 1, 0, 0));
            qy = UCS.ToACS(new Quaternion(0, 0, 1, 0));
            Quaternion qz = UCS.ToACS(new Quaternion(0, 0, 0, 1));

            vx = new Vector3d(qx.GetX(), qx.GetY(), qx.GetZ());
            yAxis = new Vector3d(qy.GetX(), qy.GetY(), qy.GetZ());
            Vector3d vZ = new Vector3d(qz.GetX(), qz.GetY(), qz.GetZ());

            Matrix3d newUcs = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, basePointResult.Value, vx, yAxis, vZ);

            double dist = basePointResult.Value.DistanceTo(endPoint);
            endPoint = new Point3d(dist, 0, 0);

            Hashtable dynamicBlockProperties = new Hashtable();
            Hashtable dynamicBlockAttributes = new Hashtable();

            dynamicBlockProperties.Add("Base Point", basePointResult);
            dynamicBlockProperties.Add("Arrow Direction", (Int16)0); // always to the right
            double rotationAngle = GeometryUtility.GetAngleFromXAxis(new Point3d(), endPoint);
            dynamicBlockProperties.Add("Arrow Rotation", rotationAngle);

            ObjectId refO = new ObjectId();
            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block ArrowMark.dwg does not exist.");
                return;
            }

            try
            {
                refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(dynamicBlockFullName, new Point3d(), dynamicBlockProperties, dynamicBlockAttributes);
            }
            catch (Exception exception)
            {
                _logger.Error("Error importing Arrow mark.", exception);
                _editorHelper.WriteMessage("Error importing Arrow mark.");
                return;
            }
          

            using (Transaction startTransaction = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                var entity = (Entity)startTransaction.GetObject(refO, OpenMode.ForWrite);
                entity.TransformBy(newUcs);
                startTransaction.Commit();
            }

            _editorHelper.CurrentUcs = ucs;
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }
    }
}
