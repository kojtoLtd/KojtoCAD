using System.Collections;
using System.IO;
using System.Reflection;
using Castle.Core.Logging;
using KojtoCAD.Mathematics.Geometry;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.Interfaces;
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

[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.SiliconJoint.SiliconJointWithSilica))]

namespace KojtoCAD.GraphicItems.SiliconJoint
{
    public class SiliconJointWithSilica
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private IFileService _fileService;

        public SiliconJointWithSilica()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _fileService = IoC.ContainerRegistrar.Container.Resolve<IFileService>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Silicon Joint
        /// </summary>
        [CommandMethod("sjs")]
        public void SiliconJointWithSilicaStart()
        {
            
            var ucs = _editorHelper.CurrentUcs;
            var xAxis = ucs.CoordinateSystem3d.Xaxis;
            var yAxis = ucs.CoordinateSystem3d.Yaxis;
            xAxis = (xAxis + yAxis) / 2.0;

            var basePointResult = _editorHelper.PromptForPoint("Pick first point :");
            if (basePointResult.Status != PromptStatus.OK)
            {
                return;
            }
            var basePoint = basePointResult.Value;
            var endPointResult = _editorHelper.PromptForPoint("Pick second point :", true, true, basePoint);
            if (endPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var endPoint = endPointResult.Value.TransformBy(GeometryUtility.GetTransforMatrixToWcs());
            basePoint = basePoint.TransformBy(GeometryUtility.GetTransforMatrixToWcs());

            _editorHelper.CurrentUcs = Matrix3d.Identity;
            var newUCS = new Matrix3d();
            var P3 = new Quaternion(0, basePoint.X + xAxis.X, basePoint.Y + xAxis.Y, basePoint.Z + xAxis.Z);
            var p3 = new Point3d(P3.GetX(), P3.GetY(), P3.GetZ());
            newUCS = GeometryUtility.GetUcs(basePoint, endPoint, p3, true);
            var dist = basePoint.DistanceTo(endPoint);
            basePoint = new Point3d(0, 0, 0);
            endPoint = new Point3d(dist, 0, 0);


            var distance = basePoint.DistanceTo(endPoint);
            var dynamicBlockProperties = new Hashtable();
            var dynamicBlockAttributes = new Hashtable();
            dynamicBlockProperties.Add("Width", distance);
            dynamicBlockProperties.Add("Base Point", basePoint);

            ObjectId refO = new ObjectId();
            try
            {
                var dynamicBlockPath =
                    _fileService.GetFile(
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            Settings.Default.dynamicBlocksDir),
                        MethodBase.GetCurrentMethod().DeclaringType.Name,
                        ".dwg");
                if (dynamicBlockPath == null)
                {
                    _editorHelper.WriteMessage("Dynamic block SiliconJointWithSilica.dwg does not exist.");
                    return;
                }
                refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                    dynamicBlockPath, basePoint, dynamicBlockProperties, dynamicBlockAttributes);
            }
            catch (Exception exception)
            {
                _logger.Error("Error importing Silicon Joint With Silica.", exception);
                _editorHelper.WriteMessage("Error importing Silicon Joint With Silica.");
                return;
            }


            using (var transaction = _drawingHelper.TransactionManager.StartTransaction())
            {
                ((Entity)transaction.GetObject(refO, OpenMode.ForWrite)).TransformBy(newUCS);
                transaction.Commit();
            }
            _editorHelper.CurrentUcs = ucs;

            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }

    }
}
