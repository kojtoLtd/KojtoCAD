using KojtoCAD.Updater.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace KojtoCAD.Updater
{
    public class AutoCadAutoloaderSettingsService : IAutoloaderSettingsService
    {
        private readonly IAppConfigurationProvider _appConfigurationProvider;
        private string _supportPathOldValue;

        public AutoCadAutoloaderSettingsService(IAppConfigurationProvider appConfigurationProvider)
        {
            _appConfigurationProvider = appConfigurationProvider;
        }
        public void EditSettingsSoTheNewVersionWillBeLoadedOnNextStartup(string newVersionDir)
        {
            var bundleDir = _appConfigurationProvider.GetKojtoCadPluginDir();
            var packageContentsFile = Path.Combine(bundleDir, "PackageContents.xml");

            var root = XDocument.Load(packageContentsFile, LoadOptions.PreserveWhitespace);
            var components = root.Element("ApplicationPackage").Elements("Components");

            // RuntimeRequirements elements
            var runtimeRequirements = components.Elements("RuntimeRequirements");
            var newVersionDirRelativePath = newVersionDir.Replace(bundleDir, string.Empty).Trim('\\');
            foreach (var runtimeRequirement in runtimeRequirements)
            {
                var supportPathAtt = runtimeRequirement.Attribute("SupportPath");
                //_supportPathOldValue = supportPathAtt.Value;
                supportPathAtt.SetValue(string.Format("./{0}", newVersionDirRelativePath));
            }

            // ComponentEntry elements
            var componentEntries = components.Elements("ComponentEntry");
            foreach (var componentEntry in componentEntries)
            {
                var moduleNameAttribute = componentEntry.Attribute("ModuleName");
                //_oldValues.Add(moduleNameAttribute.BaseUri, moduleNameAttribute.Value);
                var dllName = moduleNameAttribute.Value.Substring(moduleNameAttribute.Value.LastIndexOf('/') + 1);
                componentEntry.Attribute("ModuleName").SetValue(string.Format("./{0}/{1}", newVersionDirRelativePath,
                    dllName));
            }

            // save the file
            using (XmlTextWriter xtw = new XmlTextWriter(packageContentsFile, Encoding.UTF8))
            {
                root.Document.WriteTo(xtw);
            }
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
            if (string.IsNullOrEmpty(_supportPathOldValue))
            {
                return;
            }

            throw new NotImplementedException();
        }
    }
}
