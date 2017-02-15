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

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.GlassUnits.Triple.Triple))]

namespace KojtoCAD.BlockItems.GlassUnits.Triple
{
    public class Triple
    {
        private Hashtable _dynamicBlockProperties = new Hashtable();
        private Hashtable _dynamicBlockAttributes = new Hashtable();
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;


        public Triple()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
        }

        /// <summary>
        /// Tripple glass unit
        /// TGU_Monolitic.dwg       - triple glass not laminated
        /// TGU_Laminated.dwg       - triple glass laminated on both sides
        /// TGU_InnerLaminated.dwg  - triple glass laminated from inside the building    
        /// TGU_OuterLaminated.dwg - triple glass laminated from outside the building
        /// </summary>
        [CommandMethod("tgu")]
        public void TripleGlassUnitStart()
        {
            _dynamicBlockProperties.Clear();
            _dynamicBlockAttributes .Clear();

            ImportTripleGlassUnitForm tripleGlassUnitForm = new ImportTripleGlassUnitForm();
            if (tripleGlassUnitForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var basePointResult = _editorHelper.PromptForPoint("Pick insertion point : ");
            if (basePointResult.Status != PromptStatus.OK)
            {
                return;
            }
            _dynamicBlockProperties.Add("Base Point", basePointResult.Value);

            #region Get foil selection
            //["No Foil", "Inner Foil", "Outer Foil", "Inner Outer Foil"]
            string foil;
            if (tripleGlassUnitForm.InnerGlassHasFoil)
            {
                foil = tripleGlassUnitForm.OuterGlassHasFoil ? "Inner Outer Foil" : "Inner Foil";
            }
            else
            {
                foil = tripleGlassUnitForm.OuterGlassHasFoil ? "Outer Foil" : "No Foil";
            }
            #endregion

            #region Select the right Dynamic block
            // Select the right Dynamic block based on the options selected in the form         
            string dynamicBlockName;
            if (!tripleGlassUnitForm.InnerGlassIsLaminated)
            {
                if (!tripleGlassUnitForm.OuterGlassIsLaminated)
                {
                    dynamicBlockName = "PSYMB_TGU_Monolitic.dwg";
                    _dynamicBlockProperties.Add("Inner Glass Pane Thickness", tripleGlassUnitForm.InnerGlassThickness);
                    _dynamicBlockProperties.Add("Outer Glass Pane Thickness", tripleGlassUnitForm.OuterGlassThickness);

                    _dynamicBlockProperties.Add("Inner Gap", tripleGlassUnitForm.InnerGapThickness);
                    _dynamicBlockProperties.Add("Middle Glass Pane Thickness", tripleGlassUnitForm.MiddleGlassThickness);
                    _dynamicBlockProperties.Add("Outer Gap", tripleGlassUnitForm.OuterGapThickness);

                    _dynamicBlockProperties.Add("Length", tripleGlassUnitForm.ProfileLength);
                    _dynamicBlockProperties.Add("Foil", foil);
                }
                else
                {
                    dynamicBlockName = "PSYMB_TGU_Outer_Laminated.dwg";
                    _dynamicBlockProperties.Add("Outer External Glass Pane Thickness", tripleGlassUnitForm.OuterExternalGlassThickness);
                    _dynamicBlockProperties.Add("Outer Glass PVB Thickness", tripleGlassUnitForm.OuterPVBThickness);
                    _dynamicBlockProperties.Add("Outer Internal Glass Pane Thickness", tripleGlassUnitForm.OuterInternalGlassThickness);

                    _dynamicBlockProperties.Add("Inner Gap", tripleGlassUnitForm.InnerGapThickness);
                    _dynamicBlockProperties.Add("Middle Glass Pane Thickness", tripleGlassUnitForm.MiddleGlassThickness);
                    _dynamicBlockProperties.Add("Outer Gap", tripleGlassUnitForm.OuterGapThickness);

                    _dynamicBlockProperties.Add("Length", tripleGlassUnitForm.ProfileLength);
                    _dynamicBlockProperties.Add("Foil", foil);
                }
            }
            else
            {
                if (!tripleGlassUnitForm.OuterGlassIsLaminated)
                {
                    dynamicBlockName = "PSYMB_TGU_Inner_Laminated.dwg";
                    _dynamicBlockProperties.Add("Inner External Glass Pane Thickness", tripleGlassUnitForm.InnerExternalGlassThickness);
                    _dynamicBlockProperties.Add("Inner PVB Thickness", tripleGlassUnitForm.InnerPVBThickness);
                    _dynamicBlockProperties.Add("Inner Internal Glass Pane Thickness", tripleGlassUnitForm.InnerExternalGlassThickness);

                    _dynamicBlockProperties.Add("Inner Gap", tripleGlassUnitForm.InnerGapThickness);
                    _dynamicBlockProperties.Add("Middle Glass Pane Thickness", tripleGlassUnitForm.MiddleGlassThickness);
                    _dynamicBlockProperties.Add("Outer Gap", tripleGlassUnitForm.OuterGapThickness);

                    _dynamicBlockProperties.Add("Length", tripleGlassUnitForm.ProfileLength);
                    _dynamicBlockProperties.Add("Foil", foil);
                }
                else
                {
                    dynamicBlockName = "PSYMB_TGU_Laminated.dwg";

                    _dynamicBlockProperties.Add("Inner External Glass Pane Thickness", tripleGlassUnitForm.InnerExternalGlassThickness);
                    _dynamicBlockProperties.Add("Inner Glass PVB Thickness", tripleGlassUnitForm.InnerPVBThickness);
                    _dynamicBlockProperties.Add("Inner Internal Glass Pane Thickness", tripleGlassUnitForm.InnerInternalGlassThickness);

                    _dynamicBlockProperties.Add("Outer External Glass Pane Thickness", tripleGlassUnitForm.OuterExternalGlassThickness);
                    _dynamicBlockProperties.Add("Outer Glass PVB Thickness", tripleGlassUnitForm.OuterPVBThickness);
                    _dynamicBlockProperties.Add("Outer Internal Glass Pane Thickness", tripleGlassUnitForm.OuterExternalGlassThickness);

                    _dynamicBlockProperties.Add("Inner Gap", tripleGlassUnitForm.InnerGapThickness);
                    _dynamicBlockProperties.Add("Middle Glass Pane Thickness", tripleGlassUnitForm.MiddleGlassThickness);
                    _dynamicBlockProperties.Add("Outer Gap", tripleGlassUnitForm.OuterGapThickness);

                    _dynamicBlockProperties.Add("Length", tripleGlassUnitForm.ProfileLength);
                    _dynamicBlockProperties.Add("Foil", foil);
                }
            }
            #endregion

            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(dynamicBlockName);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block TripleGlassUnit.dwg does not exist.");
                return;
            }
            ObjectId refO = new ObjectId();
            try
            {
                refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(dynamicBlockFullName, basePointResult.Value, _dynamicBlockProperties, _dynamicBlockAttributes);
            }
            catch (Exception exception)
            {
                _logger.Error("Error importing Triple Glass Unit.", exception);
                _editorHelper.WriteMessage("Error importing Triple Glass Unit.");
                return;
            }

            using (Transaction transaction = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                Entity entity = (Entity)transaction.GetObject(refO, OpenMode.ForWrite);
                entity.TransformBy(Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem);
                transaction.Commit();
            }
            _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
    }
}
