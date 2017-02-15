using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using KojtoCAD.Attribute;
using KojtoCAD.Utilities.Interfaces;
using Microsoft.Win32;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Bricscad.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using System.Windows.Forms;
using BricscadApp;
using Bricscad.PlottingServices;
using Bricscad.ApplicationServices;
using Application = Bricscad.ApplicationServices.Application;
#endif

namespace KojtoCAD.Utilities
{
    public class UtilityClass : IUtilityClass
    {
        public string GetApplicationName()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Name;
        }

        public string GetCurrentAssemblyFileVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            var assemblyFileVersion = ((AssemblyFileVersionAttribute)attributes[0]).Version;
            return assemblyFileVersion;
        }

        public DialogResult ShowDialog(Form dialog)
        {
            return Application.ShowModalDialog(dialog);
        }

#if bcad
        public Document ReadDocument(string path, bool forReadOnly = false)
        {
            if (string.IsNullOrEmpty(path) && !File.Exists(path))
            {
                throw new ArgumentException(path);
            }
            var app = Application.AcadApplication as AcadApplication;
            if (app == null)
            {
                throw new Exception("acad app was null");
            }
            var doc = app.Documents.Open(path, forReadOnly, null);

            if (doc == null)
            {
                throw new Exception("Opened doc was null!");
            }
            app.ActiveDocument = doc;
            return Application.DocumentManager.MdiActiveDocument;
        }

        public void CloseActiveDocument(bool save)
        {
            var app = Application.AcadApplication as AcadApplication;
            app.ActiveDocument.Close(save);
        }

        public void CloseDocument(string name, bool save)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }
            var currentDoc = Application.DocumentManager.MdiActiveDocument;
            if (currentDoc.Name.Equals(name))
            {
                CloseActiveDocument(save);
                return;
            }
            foreach (Document doc in Application.DocumentManager)
            {
                if (!doc.Name.Equals(name))
                {
                    continue;
                }
                Application.DocumentManager.MdiActiveDocument = doc;
                break;
            }
            CloseActiveDocument(save);
            Application.DocumentManager.MdiActiveDocument = currentDoc;
        }

        public void CloseAndSaveDocument(Document document, string path)
        {
            document.Database.SaveAs(path, document.Database.OriginalFileVersion);
            CloseDocument(document.Name, false);
        }

        public void MinimizeWindow()
        {
            Application.MainWindow.WindowState = FormWindowState.Minimized;
        }
        public void MaximizeWindow()
        {
            Application.MainWindow.WindowState = FormWindowState.Maximized;
        }

        public int HostAppVersion()
        {
            var version = Application.Version.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries).First();
            return int.Parse(version);
        }

        public string[] PlotDevicesNames()
        {
            var plotDevices = PlotSettingsValidator.Current.GetPlotDeviceList();
            var plotDeviceName = new string[plotDevices.Count];
            plotDevices.CopyTo(plotDeviceName, 0);
            return plotDeviceName;
        } 
        public string[] PltoStylesNames()
        {
            var plotStyles = PlotSettingsValidator.Current.GetPlotStyleSheetList();
            var plotStylesNames = new string[plotStyles.Count];
            plotStyles.CopyTo(plotStylesNames, 0);
            return plotStylesNames;
        }

        public string GetCadCurVerKey()
        {
            var rkcu = Registry.CurrentUser;
            var path = @"Software\Bricsys\Bricscad\";
            using (var rk1 = rkcu.OpenSubKey(path))
            {
                path += rk1.GetValue("CurVer");
                using (var rk2 = rkcu.OpenSubKey(path))
                {
                    return path + "\\" + rk2.GetValue("CurVer");
                }
            }
        }

        // Get the Bricscad.exe location for the BricsCAD current version
        public string GetCadLocation()
        {
            var rklm = Registry.LocalMachine;
            var path = GetCadCurVerKey();
            using (var rk = rklm.OpenSubKey(path))
            {
                return (string)rk.GetValue("InstallDir");
            }
        }

        public string GetDefaultLinetypeFile()
        {
            return
                new DirectoryInfo(GetCadLocation()).GetFiles("default.lin", SearchOption.AllDirectories)
                    .First()
                    .FullName;
        }

        public IEnumerable<ObjectId> GetAllRefernecesOfBtr(ObjectId btrId, Transaction tr)
        {
            var btr = (BlockTableRecord) tr.GetObject(btrId, OpenMode.ForRead);
            return
                GetAnonymousBlocksDerivingFromBlock(btr, tr)
                    .Concat(new[] {btr})
                    .SelectMany(x => x.GetBlockReferenceIds(true, false).Cast<ObjectId>())
                    .ToArray();
        }

        private static IEnumerable<BlockTableRecord> GetAnonymousBlocksDerivingFromBlock(BlockTableRecord btr, Transaction tr)
        {
            var res = new List<BlockTableRecord>();
            var blkHand = btr.Handle;
            var blkName = btr.Name;
            var bt = (BlockTable)tr.GetObject(btr.Database.BlockTableId, OpenMode.ForRead);
            foreach (var bid in bt)
            {
                // We'll check each block in turn, to see if it has
                // XData pointing to our original block definition

                var btr2 =
                    (BlockTableRecord)tr.GetObject(bid, OpenMode.ForRead);

                // Only check blocks that don't share the name :-)

                if (btr2.Name == blkName)
                {
                    continue;
                }
                // And only check blocks with XData

                var xdata = btr2.XData;
                if (xdata == null)
                {
                    continue;
                }
                // Get the XData as an array of TypeValues and loop
                // through it

                var tvs = xdata.AsArray();
                for (var i = 0; i < tvs.Length; i++)
                {
                    // The first value should be the RegAppName

                    var tv = tvs[i];
                    if (tv.TypeCode != (int) DxfCode.ExtendedDataRegAppName)
                    {
                        continue;
                    }
                    // If it's the one we care about...

                    if ((string) tv.Value != "AcDbBlockRepBTag")
                    {
                        continue;
                    }
                    // ... then loop through until we find a
                    // handle matching our blocks or otherwise
                    // another RegAppName

                    for (var j = i + 1; j < tvs.Length; j++)
                    {
                        tv = tvs[j];
                        if (tv.TypeCode ==(int)DxfCode.ExtendedDataRegAppName)
                        {
                            // If we have another RegAppName, then
                            // we'll break out of this for loop and
                            // let the outer loop have a chance to
                            // process this section

                            i = j - 1;
                            break;
                        }

                        if (tv.TypeCode != (int) DxfCode.ExtendedDataHandle)
                        {
                            continue;
                        }
                        // If we have a matching handle...

                        if ( tv.Value.ToString() != blkHand.ToString())
                        {
                            continue;
                        }
                        // ... then we can add the block's name
                        // to the list and break from both loops
                        // (which we do by setting the outer index
                        // to the end)

                        res.Add(btr2);
                        i = tvs.Length - 1;
                        break;
                    }
                }
            }

            return res;
        }
#endif

#if !bcad
        public Document ReadDocument(string path, bool forReadOnly = false)
        {
            if (string.IsNullOrEmpty(path) && !File.Exists(path))
            {
                throw new ArgumentException(path);
            }
            return Application.DocumentManager.Open(path, forReadOnly);
        }

        public void CloseActiveDocument(bool save)
        {
            if (save)
            {
                Application.DocumentManager.MdiActiveDocument.CloseAndSave(
                    Application.DocumentManager.MdiActiveDocument.Name);
            }
            else
            {
                Application.DocumentManager.MdiActiveDocument.CloseAndDiscard();
            }
        }

        public void CloseDocument(string name, bool save)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }
            var currentDoc = Application.DocumentManager.MdiActiveDocument;
            if (currentDoc.Name.Equals(name))
            {
                CloseActiveDocument(save);
                return;
            }
            foreach (Document doc in Application.DocumentManager)
            {
                if (!doc.Name.Equals(name))
                {
                    continue;
                }
                Application.DocumentManager.MdiActiveDocument = doc;
                break;
            }
            CloseActiveDocument(save);
            Application.DocumentManager.MdiActiveDocument = currentDoc;
        }

        public void CloseAndSaveDocument(Document document, string path)
        {
            document.CloseAndSave(path);
        }

        public void MinimizeWindow()
        {
            Application.MainWindow.WindowState = Window.State.Minimized;
        }

        public void MaximizeWindow()
        {
            Application.MainWindow.WindowState = Window.State.Maximized;
        }
        public int HostAppVersion()
        {
            return Application.Version.Major;
        }
        public string[] PlotDevicesNames()
        {
            var plotDevices = PlotSettingsValidator.Current.GetPlotDeviceList();
            string[] plotDevicesNames = new string[plotDevices.Count];
            plotDevices.CopyTo(plotDevicesNames, 0);
            return plotDevicesNames;
        }

        public string[] PltoStylesNames()
        {
            var plotStyles = PlotSettingsValidator.Current.GetPlotStyleSheetList();
            string[] plotStylesNames = new string[plotStyles.Count];
            plotStyles.CopyTo(plotStylesNames, 0);
            return plotStylesNames;
        }

        public string GetCadCurVerKey()
        {
            RegistryKey rkcu = Registry.CurrentUser;
            string path = @"Software\Autodesk\AutoCAD\";
            using (RegistryKey rk1 = rkcu.OpenSubKey(path))
            {
                path += rk1.GetValue("CurVer");
                using (RegistryKey rk2 = rkcu.OpenSubKey(path))
                {
                    return path + "\\" + rk2.GetValue("CurVer");
                }
            }
        }

        // Get the acad.exe location for the AutoCAD current version
        public string GetCadLocation()
        {
            RegistryKey rklm = Registry.LocalMachine;
            string path = GetCadCurVerKey();
            using (RegistryKey rk = rklm.OpenSubKey(path))
            {
                return (string)rk.GetValue("AcadLocation");
            }
        }

        public string GetDefaultLinetypeFile()
        {
            return
                new DirectoryInfo(GetCadLocation()).GetFiles("acad.lin", SearchOption.AllDirectories)
                    .First()
                    .FullName;
        }

        public IEnumerable<ObjectId> GetAllRefernecesOfBtr(ObjectId btrId, Transaction tr)
        {
            return AttributeHelper.GetBlockReferenceIds(btrId).Cast<ObjectId>().ToList();
        }
#endif
    }

    public class UtilityClassDebug : IUtilityClass
    {
        public string GetCurrentAssemblyFileVersion()
        {
            return "1.2016.1.1";
        }

        public string GetApplicationName()
        {
            return new UtilityClass().GetApplicationName();
        }

        public DialogResult ShowDialog(Form dialog)
        {
            return new UtilityClass().ShowDialog(dialog);
        }
    }
}
