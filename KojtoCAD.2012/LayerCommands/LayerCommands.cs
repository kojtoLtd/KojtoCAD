using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
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

[assembly: CommandClass(typeof(KojtoCAD.LayerCommands.LayerCommands))]

namespace KojtoCAD.LayerCommands
{
    public class LayerCommands
    {
        private readonly Database _db = Application.DocumentManager.MdiActiveDocument.Database;
        private readonly EditorHelper _editorHelper;
        private readonly ILogger _logger = NullLogger.Instance;

        public LayerCommands()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Switch off all layers but one
        /// </summary>
        [CommandMethod("ofs")]
        public void SwitchOffAllLayersButOneStart()
        {

            PromptEntityResult selectedEntityResult = _editorHelper.PromptForObject("Select layer :", typeof(Entity), false);
            if (selectedEntityResult.Status != PromptStatus.OK)
            {
                return;
            }

            using (Transaction transaction = _db.TransactionManager.StartTransaction())
            {
                LayerTable layerTable = (LayerTable)transaction.GetObject(_db.LayerTableId, OpenMode.ForWrite);
                Entity selectedEntity = (Entity)transaction.GetObject(selectedEntityResult.ObjectId, OpenMode.ForRead);
                string entityLayerName = selectedEntity.Layer;
                foreach (ObjectId layerId in layerTable)
                {
                    LayerTableRecord layerRecord = (LayerTableRecord)transaction.GetObject(layerId, OpenMode.ForWrite, true, true);
                    layerRecord.IsOff = (layerRecord.Name != entityLayerName);
                }
                transaction.Commit();
            }


            _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Switch off a layer
        /// </summary>
        [CommandMethod("ofr")]
        public void SwitchOffLayerStart()
        {

            PromptEntityResult selectedEntityResult = _editorHelper.PromptForObject("Select layer :", typeof(Entity), false);
            if (selectedEntityResult.Status != PromptStatus.OK)
            {
                return;
            }

            using (Transaction transaction = _db.TransactionManager.StartTransaction())
            {
                //LayerTable layerTable = (LayerTable)transaction.GetObject(_db.LayerTableId, OpenMode.ForWrite);                                                                                   
                Entity selectedEntity = (Entity)transaction.GetObject(selectedEntityResult.ObjectId, OpenMode.ForRead);
                LayerTableRecord layerRecord = (LayerTableRecord)transaction.GetObject(selectedEntity.LayerId, OpenMode.ForWrite, true, true);
                layerRecord.IsOff = true;
                transaction.Commit();
            }
            _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Switch On All Layers
        /// </summary>
        [CommandMethod("ons")]
        public void SwitchOnAllLayersStart()
        {

            using (Transaction transaction = _db.TransactionManager.StartTransaction())
            {
                LayerTable layerTable = (LayerTable)transaction.GetObject(_db.LayerTableId, OpenMode.ForWrite);

                foreach (ObjectId layerId in layerTable)
                {
                    LayerTableRecord layerRecord = (LayerTableRecord)transaction.GetObject(layerId, OpenMode.ForWrite);
                    layerRecord.IsOff = false;
                }
                transaction.Commit();
            }
            _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
    }
}
