using KojtoCAD.Updater.Interfaces;
using Microsoft.Win32;
using System;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace KojtoCAD.Updater
{
    public class BricsCadAutoloaderSettingsService : IAutoloaderSettingsService
    {
        private string _oldValue;

        public void EditSettingsSoTheNewVersionWillBeLoadedOnNextStartup(string newVersionDir)
        {
            var kojtoCadKey =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                    .OpenSubKey("SOFTWARE")
                    ?.OpenSubKey("Bricsys")
                    ?.OpenSubKey("ObjectDRX")
                    ?.OpenSubKey("V17x64")
                    ?.OpenSubKey("Applications")
                    ?.OpenSubKey("KojtoCAD", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
            if (kojtoCadKey == null)
            {
                return;
            }
            var oldValue = (string) kojtoCadKey.GetValue("LOADER");
            _oldValue = oldValue;
            var newValue = $"{newVersionDir}\\{oldValue.Substring(oldValue.LastIndexOf("\\") + 1)}";
            kojtoCadKey.SetValue("LOADER", newValue, RegistryValueKind.String);
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
            if (string.IsNullOrEmpty(_oldValue))
            {
                return;
            }

            throw new NotImplementedException();
        }
    }
}