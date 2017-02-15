#if bcad
using BricscadApp;
using BricscadDb;
using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Ui.Interfaces;
using KojtoCAD.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Teigha.Runtime;
using Application = Bricscad.ApplicationServices.Application;

[assembly: CommandClass(typeof(KojtoCAD.Ui.BcadUiGenerator))]

namespace KojtoCAD.Ui
{
    public class BcadUiGenerator : UiGenerator
    {
        public BcadUiGenerator(ILogger logger, IIconManager iconsManager, IMenuSchemaProvider menuSchemaProvider)
            : base(iconsManager, menuSchemaProvider, logger)
        {
            Logger.Info(Resources.InfoUiGenerationStarted);
        }

        public BcadUiGenerator() // Do not delete. AutoCAD expects this. Otherwise UnhandledException will be thrown!
            : base()
        {

        }

        public override void GenerateUi(bool regenerateIfExists)
        {
            // 2d
            GenerateInternal(regenerateIfExists, Settings.Default.appName, DirectoriesAndFiles.BcadCuiFile);

            // 3d
            GenerateInternal(regenerateIfExists, "kojto_3d", DirectoriesAndFiles.Bcad3DCuiFile);
        }

        private void GenerateInternal(bool regenerateIfExists, string appname, string appCuiFile)
        {
            if (UiExistsInAutoCAD(appname))
            {
                if (!regenerateIfExists)
                {
                    return;
                }
                // unload
                UnloadUiFromAutoCad(appname);
            }

            // copy
            var sourceCui = appCuiFile;
            var destination = DirectoriesAndFiles.BcadCadTempDir;
            var destinationFile = Path.Combine(destination, new FileInfo(sourceCui).Name);
            File.Copy(sourceCui, destinationFile, true);

            // load
            LoadUiIntoAutoCad(destinationFile);
        }

        private void PopulateToolbarItems()
        {
            var cuiFile = DirectoriesAndFiles.BcadCuiFile;
            var cuiFileName = new FileInfo(cuiFile).Name.Replace(new FileInfo(cuiFile).Extension, string.Empty);

            var app = Application.AcadApplication as AcadApplication;
            if (app == null)
            {
                MessageBox.Show("App was null!", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var menuGroups = app.MenuGroups;
            var myGroup = menuGroups.GetMenuGroupByName(cuiFileName) ?? menuGroups.Load(cuiFile);
            var toolbars = myGroup.Toolbars;

            var commandMethods = GetCommandMethodsFromCurrentAssembly().Where(x => x.Value.Name.Length >= "Start".Length).ToArray();

            var documentElement = MenuSchemaProvider.GetMenuSchemaFile().DocumentElement;
            if (documentElement != null)
            {
                var toolbarDefinitions = documentElement.ChildNodes;

                foreach (XmlNode toolBarDefinition in toolbarDefinitions)
                {
                    if (toolBarDefinition.Attributes == null)
                    {
                        MessageBox.Show("XML file - toolbar name!", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }
                    var toolbarName = toolBarDefinition.Attributes["name"].Value;
                    var toolbar = toolbars.GetToolbarByName(toolbarName);
                    if (toolbar == null)
                    {
                        toolbar = toolbars.Add(toolbarName);
                        toolbar.Dock(AcToolbarDockStatus.acToolbarDockRight);
                    }

                    foreach (XmlNode buttonDefinition in toolBarDefinition)
                    {
                        if (buttonDefinition.Attributes == null)
                        {
                            MessageBox.Show("XML file - method name!", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
                        var buttonName = buttonDefinition.Attributes["methodName"].Value;
                        var toolbarItem = toolbar.GetToolbarItemByName(buttonName);
                        if (toolbarItem == null)
                        {
                            var buttonMethod =
                                commandMethods.FirstOrDefault(
                                    x =>
                                        x.Value.Name.Substring(0, x.Value.Name.Length - "Start".Length)
                                            .Equals(buttonName, StringComparison.InvariantCultureIgnoreCase));
                            toolbarItem = toolbar.AddToolbarButton(-1, buttonName, buttonDefinition.InnerText, buttonMethod.Key.GlobalName);
                            var smallIcon = IconsManager.GetIconFile(buttonMethod.Value.Name.Replace("Start", ""), IconSize.Small);
                            var largeIcon = IconsManager.GetIconFile(buttonMethod.Value.Name.Replace("Start", ""), IconSize.Large);
                            if (!File.Exists(smallIcon) || !File.Exists(largeIcon))
                            {
                                MessageBox.Show("Icon not found!", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                continue;
                            }
                            toolbarItem.SetBitmaps(smallIcon, largeIcon);
                        }
                    }
                }
            }
            myGroup.Save(AcMenuFileType.acMenuFileSource);
        }

        /// <summary>
        /// Regenerate classic style (not ribbon) menu 
        /// </summary>
        [CommandMethod("kojtoMenuRegen")]
        public override void RegenerateUi()
        {
            GenerateUi(true);
        }

        public override void RemoveUi()
        {
            var cuiFile = DirectoriesAndFiles.BcadCuiFile;
            var cuiFileName = new FileInfo(cuiFile).Name.Replace(new FileInfo(cuiFile).Extension, string.Empty);

            var app = Application.AcadApplication as AcadApplication;
            var menuGroups = app.MenuGroups;
            var myGroup = menuGroups.GetMenuGroupByName(cuiFileName);
            if (myGroup != null)
            {
                myGroup.Unload();
            }
        }

        protected override void LoadUiIntoAutoCad(string uiFile)
        {
            var app = Application.AcadApplication as AcadApplication;
            var menuGroups = app.MenuGroups;
            var menuGroup = menuGroups.GetMenuGroupByName(uiFile);
            if (menuGroup == null)
            {
                menuGroups.Load(uiFile);
            }
        }

        protected override void UnloadUiFromAutoCad(string uiFile)
        {
            var app = Application.AcadApplication as AcadApplication;
            var menuGroups = app.MenuGroups;
            var menuGroup = menuGroups.GetMenuGroupByName(uiFile);
            if (menuGroup != null)
            {
                menuGroup.Unload();
            }
        }

        protected override bool UiExistsInAutoCAD(string uiFile)
        {
            var app = Application.AcadApplication as AcadApplication;
            var menuGroups = app.MenuGroups;
            var menuGroup = menuGroups.GetMenuGroupByName(uiFile);
            return menuGroup != null;
        }
    }
}

#endif
