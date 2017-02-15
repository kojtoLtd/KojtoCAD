using System;
using System.IO;
using KojtoCAD.Updater.Interfaces;

namespace KojtoCAD.Updater
{
    public class AppConfigurationProvider : IAppConfigurationProvider
    {
        public string GetBlobConnectionString()
        {
            return Properties.Settings.Default.BlobConnectionString;
        }

        public string GetBlobContainerName()
        {
            return Properties.Settings.Default.BlobContainerName;
        }

        public string GetKojtoCadVirtualDirectoryName()
        {
            return Properties.Settings.Default.NewVersionsVirtualDirectory;
        }

        public string GetProgramFilesDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        public string GetKojtoCadPluginDir()
        {
            return Path.Combine(GetProgramFilesDir(), @"Autodesk\ApplicationPlugins\KojtoCad.bundle");
        }
    }
}
