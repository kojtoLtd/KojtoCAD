using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
using System.Collections;
using System.Reflection;
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

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.Marks.TitleMark))]

namespace KojtoCAD.BlockItems.Marks
{
    public class TitleMark
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public TitleMark()
        {
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Title Mark
        /// </summary>
        [CommandMethod("tm")]
        public void TitleMarkStart()
        {
            Matrix3d ucs = _editorHelper.CurrentUcs;
            Point3d pointOfOrigin = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d.Origin;
            _editorHelper.CurrentUcs = Matrix3d.Identity;

            var basePointResult = _editorHelper.PromptForPoint("Pick insertion point : ");
            if (basePointResult.Status != PromptStatus.OK)
            {
                return;
            }

            Vector3d displacementVector = pointOfOrigin.GetVectorTo(basePointResult.Value);

            Hashtable dynamicBlockProperties = new Hashtable();
            Hashtable dynamicBlockAttributes = new Hashtable();
            const double titleMarkLenght = 50;
            dynamicBlockProperties.Add("Title Line Length", titleMarkLenght);
            dynamicBlockProperties.Add("Base Point",  basePointResult.Value);

            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block TitleMark.dwg does not exist.");
                return;
            }
            ObjectId refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                dynamicBlockFullName,
                 new Point3d(0, 0, 0),
                dynamicBlockProperties,
                dynamicBlockAttributes);

            using (Transaction transaction = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                var entity = (Entity)transaction.GetObject(refO, OpenMode.ForWrite);
                entity.TransformBy(ucs);
                entity.TransformBy(Matrix3d.Displacement(displacementVector));
                transaction.Commit();
            }
            _editorHelper.CurrentUcs = ucs;

            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }
    }
}
