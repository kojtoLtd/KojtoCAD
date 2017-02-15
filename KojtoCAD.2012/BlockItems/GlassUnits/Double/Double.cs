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

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.GlassUnits.Double.Double))]

namespace KojtoCAD.BlockItems.GlassUnits.Double
{
    public class Double
    {
        private readonly Hashtable _dynamicBlockProperties = new Hashtable();
        private readonly Hashtable _dynamicBlockAttributes = new Hashtable();
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;


        public Double()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
        }

        /// <summary>
        /// Double Glass Unit
        /// PSYMB_DGU_Monolitic.dwg       - double glass not laminated
        /// PSYMB_DGU_Laminated.dwg       - double glass laminated on both sides
        /// PSYMB_DGU_InnerLaminated.dwg  - double glass laminated from inside the building    
        /// PSYMB_DGU_OuterLaminated.dwg - double glass laminated from outside the building
        /// </summary>
        [CommandMethod("dgu")]
        public void DoubleGlassUnitStart()
        {
            _dynamicBlockProperties.Clear();
            _dynamicBlockAttributes.Clear();

            var doubleGlassUnitForm = new ImportDoubleGlassUnitForm();
            if (doubleGlassUnitForm.ShowDialog() != DialogResult.OK)
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
            if (doubleGlassUnitForm.InnerGlassHasFoil)
            {
                foil = doubleGlassUnitForm.OuterGlassHasFoil ? "Inner And Outer Foil" : "Inner Foil";
            }
            else  // if InnerGlass has no foil
            {
                foil = doubleGlassUnitForm.OuterGlassHasFoil ? "Outer Foil" : "No Foil";
            }
            #endregion
            

            #region Select the right Dynamic block
            // Select the right Dynamic block based on the options selected in the form         
            string dynamicBlockName;
            if (!doubleGlassUnitForm.InnerGlassIsLaminated)
            {
                if (!doubleGlassUnitForm.OuterGlassIsLaminated)
                {
                    dynamicBlockName = "PSYMB_DGU_Monolitic.dwg";
                    _dynamicBlockProperties.Add("Inner Glass Pane Thickness", doubleGlassUnitForm.InnerGlassThickness);
                    _dynamicBlockProperties.Add("Gap", doubleGlassUnitForm.GapThickness);
                    _dynamicBlockProperties.Add("Outer Glass Pane Thickness", doubleGlassUnitForm.OuterGlassThickness);
                    _dynamicBlockProperties.Add("Length", doubleGlassUnitForm.ProfileLength);
                    _dynamicBlockProperties.Add("Foil", foil);
                }
                else
                {
                    dynamicBlockName = "PSYMB_DGU_Outer_Laminated.dwg";
                    _dynamicBlockProperties.Add("Outer External Glass Pane Thickness", doubleGlassUnitForm.OuterExternalGlassThickness);
                    _dynamicBlockProperties.Add("Outer Glass PVB Thickness", doubleGlassUnitForm.OuterPVBThickness);
                    _dynamicBlockProperties.Add("Outer Internal Glass Pane Thickness", doubleGlassUnitForm.OuterInternalGlassThickness);

                    _dynamicBlockProperties.Add("Inner Glass Pane Thickness", doubleGlassUnitForm.InnerGlassThickness);
                    _dynamicBlockProperties.Add("Gap", doubleGlassUnitForm.GapThickness);
                    _dynamicBlockProperties.Add("Length", doubleGlassUnitForm.ProfileLength);
                    _dynamicBlockProperties.Add("Foil", foil);
                }
            }
            else
            {
                if (!doubleGlassUnitForm.OuterGlassIsLaminated)
                {
                    dynamicBlockName = "PSYMB_DGU_Inner_Laminated.dwg";
                    _dynamicBlockProperties.Add("Inner External Glass Pane Thickness", doubleGlassUnitForm.InnerExternalGlassThickness);
                    _dynamicBlockProperties.Add("Inner PVB Thickness", doubleGlassUnitForm.InnerPVBThickness);
                    _dynamicBlockProperties.Add("Inner Internal Glass Pane Thickness", doubleGlassUnitForm.InnerExternalGlassThickness);

                    _dynamicBlockProperties.Add("Outer Glass Pane Thickness", doubleGlassUnitForm.OuterGlassThickness);
                    _dynamicBlockProperties.Add("Gap", doubleGlassUnitForm.GapThickness);
                    _dynamicBlockProperties.Add("Length", doubleGlassUnitForm.ProfileLength);
                    _dynamicBlockProperties.Add("Foil", foil);
                }
                else
                {
                    dynamicBlockName = "PSYMB_DGU_Laminated.dwg";

                    _dynamicBlockProperties.Add("Inner External Glass Pane Thickness", doubleGlassUnitForm.InnerExternalGlassThickness);
                    _dynamicBlockProperties.Add("Inner Glass PVB Thickness", doubleGlassUnitForm.InnerPVBThickness);
                    _dynamicBlockProperties.Add("Inner Internal Glass Pane Thickness", doubleGlassUnitForm.InnerInternalGlassThickness);

                    _dynamicBlockProperties.Add("Outer External Glass Pane Thickness", doubleGlassUnitForm.OuterExternalGlassThickness);
                    _dynamicBlockProperties.Add("Outer Glass PVB Thickness", doubleGlassUnitForm.OuterPVBThickness);
                    _dynamicBlockProperties.Add("Outer Internal Glass Pane Thickness", doubleGlassUnitForm.OuterExternalGlassThickness);

                    _dynamicBlockProperties.Add("Gap", doubleGlassUnitForm.GapThickness);
                    _dynamicBlockProperties.Add("Length", doubleGlassUnitForm.ProfileLength);
                    _dynamicBlockProperties.Add("Foil", foil);
                }
            }
            #endregion

            string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(dynamicBlockName);
            if (dynamicBlockFullName == null)
            {
                _editorHelper.WriteMessage("Dynamic block DoubleGlassUnit.dwg does not exist.");
                return;
            }
            ObjectId refO = new ObjectId();
            try
            {
                refO = _drawingHelper.ImportDynamicBlockAndFillItsProperties(dynamicBlockFullName, basePointResult.Value, _dynamicBlockProperties, _dynamicBlockAttributes);
            }
            catch (Exception exception)
            {
                _logger.Error("Error importing Double Glass Unit.", exception);
                _editorHelper.WriteMessage("Error importing Double Glass Unit.");
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
