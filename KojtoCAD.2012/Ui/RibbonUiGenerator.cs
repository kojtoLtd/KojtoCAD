using Castle.Core.Logging;
using KojtoCAD.Ui.Interfaces;

namespace KojtoCAD.Ui
{
    public class RibbonUiGenerator : UiGenerator
    {
        private ILogger _logger = NullLogger.Instance;

        public RibbonUiGenerator(IIconManager iconsProvider, IMenuSchemaProvider menuSchemaProvider, ILogger logger)
            : base(iconsProvider, menuSchemaProvider, logger)
        {
            _logger = logger;
        }

        public override void GenerateUi(bool regenerateIfExists)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveUi()
        {
            throw new System.NotImplementedException();
        }

        protected override void LoadUiIntoAutoCad(string uiFile)
        {
            throw new System.NotImplementedException();
        }

        protected override void UnloadUiFromAutoCad(string uiFile)
        {
            throw new System.NotImplementedException();
        }

        public override void RegenerateUi()
        {
            throw new System.NotImplementedException();
        }

        protected override bool UiExistsInAutoCAD(string uiFile)
        {
            throw new System.NotImplementedException();
        }
    }
}