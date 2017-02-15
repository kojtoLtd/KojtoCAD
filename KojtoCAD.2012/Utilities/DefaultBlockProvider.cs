using System.IO;
using System.Reflection;
using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Properties;
using KojtoCAD.Utilities.Interfaces;

namespace KojtoCAD.Utilities
{
    public class DefaultBlockProvider : IBlockDrawingProvider
    {
        private readonly IFileService _fileService;
        private ILogger _logger = NullLogger.Instance;

        public DefaultBlockProvider(IFileService fileService)
        {
            _fileService = fileService;
        }

        public ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public string GetBlockFile(string dynamicBlockName)
        {
            string executingAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dynamicBlocksFolder = Path.Combine(executingAssemblyLocation, Settings.Default.dynamicBlocksDir);

            if (!Directory.Exists(dynamicBlocksFolder))
            {
                _logger.Error("Dynamic blocks folder does not exist : " + dynamicBlocksFolder);
                throw new DirectoryNotFoundException("Dynamic blocks folder does not exist.");
            }
            return _fileService.GetFile(dynamicBlocksFolder, dynamicBlockName.Replace(".dwg",""), ".dwg");
        }

    }
}