using System.IO;
using System.Reflection;
using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Ui.Interfaces;
using KojtoCAD.Utilities.ErrorReporting.Exceptions;
using KojtoCAD.Utilities.Interfaces;
using System;

namespace KojtoCAD.Ui
{
    public class DefaultIconManager : IIconManager
    {
        private readonly IFileService _fileService;
        private ILogger _logger = NullLogger.Instance;

        public DefaultIconManager(IFileService fileService, ILogger logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        public string GetIconFile(string iconName, IconSize iconSize)
        {
            string executingAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string iconsFolder = Path.Combine(executingAssemblyLocation, Settings.Default.iconsDir);

            if (!Directory.Exists(iconsFolder))
            {
                _logger.Error(Resources.ErrorIconsDirectoryDoesNotExist + iconsFolder);
                throw new DirectoryNotFoundException(Resources.ErrorIconsDirectoryDoesNotExist);
            }
            string iconFile;
            try
            {
                iconFile = _fileService.GetFile(iconsFolder.Replace("Icons\\", ""), iconName  + iconSize.ToString() , ".bmp");
                if (iconFile == null)
                {
                    throw new FileNotFoundException();
                }
            }
            catch (Exception generalException)
            {
                throw new IconProviderException(Resources.ErrorGettingIcon + iconName, generalException);
            }
            
            return iconFile;
        }

        public void DeleteIconFile(string iconFileFullPath)
        {
            try
            {
                File.Delete(iconFileFullPath);
            }
            catch (IOException exception)
            {
                throw new IOException(Resources.ErrorDeletingIcon, exception);
            }
        }
    }
}