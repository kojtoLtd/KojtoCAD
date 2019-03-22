using KojtoCAD.Updater.Interfaces;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace KojtoCAD.Updater
{
    public class BricsCadAutoloaderSettingsService : IAutoloaderSettingsService
    {
        private readonly int[] _supportedVersions = { 16, 17, 18, 19, 20 };

        public void EditSettingsSoTheNewVersionWillBeLoadedOnNextStartup(string newVersionDir)
        {
            var installedVersionsKeys = _supportedVersions.Select(x => RegistryKey
                    .OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                    .OpenSubKey("SOFTWARE")
                    ?.OpenSubKey("Bricsys")
                    ?.OpenSubKey("ObjectDRX")
                    ?.OpenSubKey($"V{x}x64")
                    ?.OpenSubKey("Applications")
                    ?.OpenSubKey("KojtoCAD", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
                .Where(x => x != null).ToList();

            installedVersionsKeys.ForEach(x =>
            {
                var oldValue = (string)x.GetValue("LOADER");
                var dllName =
                    oldValue.Substring(oldValue.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase) + 1);
                var newValue = $"{newVersionDir}\\{dllName}";
                x.SetValue("LOADER", newValue, RegistryValueKind.String);
            });
        }

        public Task EditSettingsSoTheNewVersionWillBeLoadedOnNextStartupAsync(string newVersionDir)
        {
            return Task.Run(() =>
            {
                EditSettingsSoTheNewVersionWillBeLoadedOnNextStartup(newVersionDir);
            });
        }

        public void RevertOldValues()
        {
            throw new NotImplementedException();
        }
    }
}