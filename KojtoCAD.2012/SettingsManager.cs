using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.Interfaces;
using System.Reflection;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Application = Bricscad.ApplicationServices.Application;
using Exception = Teigha.Runtime.Exception;
#endif

[assembly: CommandClass(typeof(KojtoCAD.SettingsManager))]

namespace KojtoCAD
{
    public class SettingsManager
    {
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IFileService _fileService;
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        public SettingsManager()
        {
            _fileService = IoC.ContainerRegistrar.Container.Resolve<IFileService>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
        }

        /// <summary>
        /// Load default KojtoCAD settings
        /// for Layers, Dimension Styles, Line Types
        /// We are doing this by importing and erasing
        /// as block Templates\all.dwg 
        /// </summary>
        [CommandMethod("kojtosettings")]
        public void LoadSettingsStart()
        {
            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            DocumentHelper drawingManager = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);

            if (!doc.UserData.Contains("SETTINGS_LOADED"))
            {
                // Get all.dwg full directory path
                string sourceDrawing = _fileService.GetFile(
                    System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Settings.Default.templatesDir),
                    "all", ".dwg");
                if (sourceDrawing == null)
                {
                    _editorHelper.WriteMessage("Settings file \"all.dwg\" does not exist.");
                    return;
                }
                // Import all.dwg
                drawingManager.ImportDwgAsBlock(sourceDrawing, new Point3d(0, 0, 0));

                string settingsBlockName = sourceDrawing.Remove(0,
                                                                sourceDrawing.LastIndexOf("\\",
                                                                                          System.StringComparison
                                                                                                .Ordinal) + 1);
                settingsBlockName = settingsBlockName.Substring(0, settingsBlockName.Length - 4);
                // remove the extension

                // Find the block and its reference and erase it so only the settings remain in the dwg
                using (doc.LockDocument())
                {
                    using (Transaction transaction = db.TransactionManager.StartTransaction())
                    {
                        var blockTable = (BlockTable) transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                        ObjectId settingsBlockId = drawingManager.GetErasedButResidentTableRecordId(
                            blockTable.ObjectId, settingsBlockName);

                        try
                        {
                            var blockTableRecord =
                                (BlockTableRecord) transaction.GetObject(settingsBlockId, OpenMode.ForWrite, true);
                            ObjectIdCollection blockReferenceIds = blockTableRecord.GetBlockReferenceIds(true, false);
                            foreach (ObjectId objId in blockReferenceIds)
                            {
                                var entity = (Entity) transaction.GetObject(objId, OpenMode.ForWrite);
                                entity.Erase(true);
                            }
                            blockTableRecord.Erase(true);
                            blockTableRecord.DowngradeOpen();
                        }
                        catch (Exception exception)
                        {
                            _logger.Error("Seetings failed to load.", exception);
                        }
                        transaction.Commit();
                    }

                    doc.UserData.Add("SETTINGS_LOADED", 1);
                }
                _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

}
}
