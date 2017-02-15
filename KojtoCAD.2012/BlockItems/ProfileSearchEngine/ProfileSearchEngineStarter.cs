using Castle.Core.Logging;
using KojtoCAD.Utilities;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif

[assembly : CommandClass(typeof(KojtoCAD.BlockItems.ProfileSearchEngine.ProfileSearchEngineStarter))]

namespace KojtoCAD.BlockItems.ProfileSearchEngine
{
    public class ProfileSearchEngineStarter
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;

        public ProfileSearchEngineStarter()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Profile Search
        /// </summary>
        [CommandMethod("ib")]
        public void ProfileSearchEngineStart()
        {
            var importBlockForm = new ProfileSearchEngine();
            if (importBlockForm.ShowDialog() == DialogResult.OK)
            {
                var insertionPointResult = _editorHelper.PromptForPoint("Pick the base point of the block.");
                if (insertionPointResult.Status != PromptStatus.OK)
                {
                    return;
                }

                _drawingHelper.ImportDwgAsBlock(importBlockForm.DwgFileFullPath, insertionPointResult.Value);
            }
            importBlockForm.Close();
            _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
    }
}


