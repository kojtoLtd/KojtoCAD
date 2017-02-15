using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
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
[assembly: CommandClass(typeof(KojtoCAD.BlockItems.Marks.PositionMark))]

namespace KojtoCAD.BlockItems.Marks
{
    

    public class PositionMark
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public PositionMark()
        {
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Position Mark
        /// </summary>
        [CommandMethod("pm")]
        public void PositionMarkStart()
        {
            Matrix3d ucs = _editorHelper.CurrentUcs;
            Point3d pO = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Origin;

            PromptPointResult firstPointResult = _editorHelper.PromptForPoint("Pick arrow point : ", false, false);
            if (firstPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            var firstPoint = firstPointResult.Value.TransformBy(GeometryUtility.GetTransforMatrixToWcs());


            PromptPointResult secondPointResult = _editorHelper.PromptForPoint("Pick pos number sign point : ", true, true, firstPoint);
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            var secondPoint = secondPointResult.Value.TransformBy(GeometryUtility.GetTransforMatrixToWcs());

            PromptIntegerResult posNumberResult = _editorHelper.PromptForInteger("Enter POS integer number :");
            if (posNumberResult.Status != PromptStatus.OK)
            {
                return;
            }

            _editorHelper.CurrentUcs = Matrix3d.Identity;
            Quaternion S = new Quaternion(0, secondPoint.X, secondPoint.Y, secondPoint.Z);
            Quaternion F = new Quaternion(0, firstPoint.X, firstPoint.Y, firstPoint.Z);

            Vector3d v1 = pO.GetVectorTo(secondPoint);
            Vector3d v = secondPoint.GetVectorTo(new Point3d(0, 0, 0));
            secondPoint = secondPoint.TransformBy(Matrix3d.Displacement(v));
            firstPoint = firstPoint.TransformBy(Matrix3d.Displacement(v));

            Quaternion ucsOrigin = new Quaternion(0, pO.X, pO.Y, pO.Z);
            Quaternion Xaxe = new Quaternion(0, ucs.CoordinateSystem3d.Xaxis.X, ucs.CoordinateSystem3d.Xaxis.Y, ucs.CoordinateSystem3d.Xaxis.Z);
            Quaternion Yaxe = new Quaternion(0, ucs.CoordinateSystem3d.Yaxis.X, ucs.CoordinateSystem3d.Yaxis.Y, ucs.CoordinateSystem3d.Yaxis.Z);
            Xaxe = ucsOrigin + Xaxe;
            Yaxe = ucsOrigin + Yaxe;
            UCS UCS = new UCS(ucsOrigin, Xaxe, Yaxe);
            Quaternion FS = UCS.FromACS(S) - UCS.FromACS(F);
            double ang = FS.angToX();
            if (FS.GetY() < 0) { ang *= -1; }


            Hashtable dynamicBlockProperties = new Hashtable();
            Hashtable dynamicBlockAttributes = new Hashtable();
            dynamicBlockProperties.Add("Base Point", secondPoint);
            dynamicBlockProperties.Add("Distance", secondPoint.DistanceTo(firstPoint));

            dynamicBlockProperties.Add("Angle", ang);

            dynamicBlockAttributes.Clear();
            dynamicBlockAttributes.Add("POS", posNumberResult.Value);

            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block PositionMark.dwg does not exist.");
                return;
            }
            ObjectId refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                dynamicBlockFullName,
                secondPoint,
                dynamicBlockProperties,
                dynamicBlockAttributes);

            using (Transaction acTrans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                Entity ent = (Entity)acTrans.GetObject(refO, OpenMode.ForWrite);
                ent.TransformBy(ucs);
                ent.TransformBy(Matrix3d.Displacement(v1));
                acTrans.Commit();
            }
            _editorHelper.CurrentUcs = ucs;

            _logger.Info(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }
    }
}
