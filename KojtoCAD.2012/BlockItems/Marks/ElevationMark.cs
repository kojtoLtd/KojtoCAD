using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
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

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.Marks.ElevationMark))]

namespace KojtoCAD.BlockItems.Marks
{
    public class ElevationMark
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private static ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public ElevationMark()
        {
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Elevation Mark
        /// </summary>
        [CommandMethod("em")]
        public void ElevationMarkStart()
        {
            var firstPointResult = _editorHelper.PromptForPoint("Pick sign first corner : ");
            if (firstPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var secondPointResult = _editorHelper.PromptForPoint("Pick sign opposite corner : ", true, true, firstPointResult.Value);
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var basePointResult = _editorHelper.PromptForPoint("Pick arrow point : ", true, true, secondPointResult.Value);
            if (basePointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var firstPoint = firstPointResult.Value;
            var secondPoint = secondPointResult.Value;

            Vector3d mV = basePointResult.Value.GetVectorTo(new Point3d(0, 0, 0));

            Point3d basePoint = basePointResult.Value.TransformBy(Matrix3d.Displacement(mV));
            firstPoint = firstPoint.TransformBy(Matrix3d.Displacement(mV));
            secondPoint = secondPoint.TransformBy(Matrix3d.Displacement(mV));

            var distanceSecPtToBasePt = secondPoint.DistanceTo(basePoint);
            var topRightCorner = new Point3d(basePoint.X - distanceSecPtToBasePt+10, basePoint.Y+10,basePoint.Z);
            var vectorTrans = secondPoint.GetVectorTo(topRightCorner);

            double x = Math.Abs(firstPoint.X - secondPoint.X);
            double y = Math.Abs(firstPoint.Y - secondPoint.Y);
            if (x < 30) { x = 30; }
            if (y < 30) { y = 30; }

            Hashtable dynamicBlockProperties = new Hashtable();
            Hashtable dynamicBlockAttributes = new Hashtable();
            
            dynamicBlockProperties.Add("X Distance", x); 
            dynamicBlockProperties.Add("Y Distance", y); 
            dynamicBlockProperties.Add("Distance", secondPoint.DistanceTo(basePoint));
            dynamicBlockProperties.Add("Angle", 0.0/*ang*/ );

            Matrix3d ucs = _editorHelper.CurrentUcs;
            //_editorHelper.CurrentUcs = Matrix3d.Identity;
            
            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block ElevationMark.dwg does not exist.");
                return;
            }

            ObjectId refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                dynamicBlockFullName,
                basePoint, 
                dynamicBlockProperties, 
                dynamicBlockAttributes);

            using (Transaction acTrans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                Entity ent = (Entity)acTrans.GetObject(refO, OpenMode.ForWrite);
                try
                {
                    ent.TransformBy(ucs);
                    ent.TransformBy(Matrix3d.Displacement(-mV - vectorTrans));
                }
                catch
                {
                    MessageBox.Show("The three Points lie on a Line", "E R R O R");
                }
                acTrans.Commit();
            }
            _editorHelper.CurrentUcs = ucs;
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }
    }
}
