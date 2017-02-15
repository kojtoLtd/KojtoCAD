using System;
using System.Drawing;
using System.IO;
using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Ui.Interfaces;

namespace KojtoCAD.Ui
{
    internal class ResourceManagerBasedIconManager : IIconManager
    {

        private ILogger _logger = NullLogger.Instance;

        public ResourceManagerBasedIconManager(ILogger logger)
        {
            _logger = logger;
        }

        protected ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public string GetIconFile(string iconName, IconSize iconSize)
        {
            string iconInTempDir = ExtractIconFromResourcesToTempDir(iconName, iconSize);
            return iconInTempDir;
        }

        private string ExtractIconFromResourcesToTempDir(string iconName, IconSize iconSize)
        {
            string iconNameInResource = iconName + iconSize.ToString();
            Bitmap bmp = (Bitmap) Resources.ResourceManager.GetObject(iconNameInResource);


            string iconsTempDir = Path.Combine(Path.GetTempPath(), Settings.Default.appDir, Settings.Default.iconsDir);
            string iconFileToSave = Path.Combine(iconsTempDir, iconNameInResource + ".bmp");

            if (!Directory.Exists(iconsTempDir))
            {
                try
                {
                    Directory.CreateDirectory(iconsTempDir);
                }
                catch (Exception exception)
                {
                    _logger.Error("Unable to create icons temp directory.", exception);
                    throw;
                }

            }

            if (!File.Exists(iconFileToSave))
            {
                try
                {
                    bmp.Save(iconFileToSave);

                }
                catch (Exception exception)
                {
                    _logger.Error("Unable to save icon in icons temp directory.", exception);
                    throw;
                }
            }

            return iconFileToSave;
        }

        public void DeleteIconFile(string iconFileFullPath)
        {
            if (!File.Exists(iconFileFullPath))
            {
                return;
            }

            try
            {
                File.Delete(iconFileFullPath);
            }
            catch (IOException exception)
            {
                _logger.Error("Cuold not delete icon.", exception);
                throw new IOException("Cuold not delete icon.", exception);
            }
        }
    }
}
