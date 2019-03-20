using System;
using System.IO;
using KojtoCAD.Updater.Interfaces;
using System.Configuration;
using System.Reflection;

namespace KojtoCAD.Updater
{
    public class AppConfigurationProvider : IAppConfigurationProvider
    {
        private readonly Configuration _config;

        public AppConfigurationProvider()
        {
            _config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
        }
        public string GetBlobConnectionString()
        {
            return _config.AppSettings.Settings["BlobConnectionString"].Value;
        }

        public string GetBlobContainerName()
        {
            return _config.AppSettings.Settings["BlobContainerName"].Value;
        }

        public string GetKojtoCadVirtualDirectoryName()
        {
            return _config.AppSettings.Settings["NewVersionsVirtualDirectory"].Value;
        }

        public string GetProgramFilesDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        public string GetKojtoCadPluginDir()
        {
            return Path.Combine(GetProgramFilesDir(), @"Autodesk\ApplicationPlugins\KojtoCad.bundle");
        }

        public bool WebTrackerIsEnabled()
        {
            return Properties.Settings.Default.WebTrackerIsEnabled;
        }
    }
}
