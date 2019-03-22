using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using KojtoCAD.Properties;

namespace KojtoCAD.Utilities
{
    public static class DirectoriesAndFiles
    {
        public static readonly string KojtoCadTempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", Settings.Default.appDir);
        public static readonly string KCad3DCuixFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Settings.Default.templatesDir, "KOJTO_3D.cuix");

        public static readonly string SuretyFile2D = KojtoCadTempDir + "\\" + Settings.Default.appName + "Surety.xml";
        public static readonly string SuretyFile3D = KojtoCadTempDir + "\\" + Settings.Default.appName + "Surety3D.xml";

        public static readonly string BcadCuiFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Settings.Default.templatesDir, "kojto.cui");
        public static readonly string Bcad3DCuiFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Settings.Default.templatesDir, "kojto_3D.cui");
        public static readonly string KCad3DMenuSchema = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Settings.Default.templatesDir, "KojtoCAD_3D.xml");
        public static string BcadCadTempDir
        {
            get
            {
                // version 16, 17, 18 etc...
                var bricsCadVersion = Process
                    .GetProcesses().First(x => x.ProcessName.Equals("BRICSCAD", StringComparison.OrdinalIgnoreCase)).MainModule.FileVersionInfo.ProductMajorPart;
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    $@"Bricsys\BricsCAD\V{bricsCadVersion}x64\en_US\Support");
            }
        }
    }
}
