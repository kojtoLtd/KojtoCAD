using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
using System.Collections;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif


[assembly: CommandClass(typeof( KojtoCAD.BlockItems.GlassUnits.Single.Single))]


namespace KojtoCAD.BlockItems.GlassUnits.Single
{
    public class Single
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;


        public Single()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
        }

        /// <summary>
        /// Single Glass Unit
        /// </summary>
        [CommandMethod("sgu")]
        public void SingleGlassUnitStart()
        {
            var singleGlassUnitForm = new ImportSingleGlassUnitForm();
            if (singleGlassUnitForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var basePointResult = _editorHelper.PromptForPoint("Pick insertion point : ");
            if (basePointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var dynamicBlockProperties = new Hashtable();
            var dynamicBlockAttributes = new Hashtable();
            dynamicBlockProperties.Add("Base Point", basePointResult.Value);

            // Select the right Dynamic block based on the options selected in the form         
            string dynamicBlockName = "";
            if (singleGlassUnitForm.IsLaminated)
            {
                dynamicBlockName = "PSYM_SGU_Laminated.dwg";
                dynamicBlockProperties.Add("Inner Glass Pane Thickness", singleGlassUnitForm.InnerGlassThickness);
                dynamicBlockProperties.Add("Outer Glass Pane Thickness", singleGlassUnitForm.OuterGlassThickness);
                dynamicBlockProperties.Add("PVB Thickness", singleGlassUnitForm.PVBLayersThickness);
                dynamicBlockProperties.Add("Length", singleGlassUnitForm.ProfileLenght);
            }
            else
            {
                dynamicBlockName = "PSYMB_SGU.dwg";
                dynamicBlockProperties.Add("Glass Pane Thickness", singleGlassUnitForm.InnerGlassThickness);
                dynamicBlockProperties.Add("Length", singleGlassUnitForm.ProfileLenght);
            }

            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(dynamicBlockName);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block SingleGlassUnit.dwg does not exist.");
                return;
            }
            ObjectId refO = new ObjectId();
            try
            {
                refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(dynamicBlockFullName, basePointResult.Value, dynamicBlockProperties, dynamicBlockAttributes);
            }
            catch (Exception exception)
            {
                _logger.Error("Error importing Single Glass Unit.", exception);
                _editorHelper.WriteMessage("Error importing Single Glass Unit.");
                return;
            }

            using (var transaction = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                var entity = (Entity)transaction.GetObject(refO, OpenMode.ForWrite);
                entity.TransformBy(Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem);
                transaction.Commit();
            }
            _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
    }
}
