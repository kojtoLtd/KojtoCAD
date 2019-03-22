#if !bcad
using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Ui.Interfaces;
using KojtoCAD.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(KojtoCAD.Ui.ClassicUiGenerator))]

namespace KojtoCAD.Ui
{
    public class ClassicUiGenerator : UiGenerator
    {
        private readonly Lazy<EditorHelper> _editorHelper = new Lazy<EditorHelper>(() => new EditorHelper(Application.DocumentManager.MdiActiveDocument), true);

        public ClassicUiGenerator(ILogger logger,IIconManager iconsManager, IMenuSchemaProvider menuSchemaProvider)
            : base(iconsManager, menuSchemaProvider, logger)
        {
            Logger.Info(Resources.InfoUiGenerationStarted);
        }

        public ClassicUiGenerator() // Do not delete. AutoCAD expects this. Otherwise UnhandledException will be thrown!
            : base()
        {

        }

        /// <summary>
        /// Generate user interface.
        /// </summary>
        /// <param name="regenerateIfExists">
        /// Indicates wheter to regenerate the user interface if it is already generated and loaded into Autocad.
        /// </param>
        public override void GenerateUi(bool regenerateIfExists)
        {
            if (!this.CheckAutoCadVersion(17, 0))
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(Resources.WarningUiVersion);
                return;
            }

            string kojtoCadCuiDir = DirectoriesAndFiles.KojtoCadTempDir;
            try
            {
                this.CreateUiFolderIfNotExists(kojtoCadCuiDir);
            }
            catch (IOException exception)
            {
                Logger.Error(Resources.ErrorCreatingUiDirectory, exception);
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(Resources.ErrorCreatingUiDirectory);
            }
            string kojtoCadCuiFile = kojtoCadCuiDir + "\\" + Settings.Default.appName + this.GetCuiExtensionForCurrentVersion();

            if (this.GetMainCustomizationSection().PartialCuiFiles.Contains(kojtoCadCuiFile) && regenerateIfExists)
            {
                this.UnloadUiFromAutoCad(kojtoCadCuiFile);
            }
            else if (this.GetMainCustomizationSection().PartialCuiFiles.Contains(kojtoCadCuiFile) && !regenerateIfExists )
            {
                return;
            }

            if (File.Exists(kojtoCadCuiFile))
            {
                File.Delete(kojtoCadCuiFile);
            }

            try
            {
                var kojtoCui = 
                    BuildMenu(MenuSchemaProvider.GetMenuSchemaFile(), this.GetCommandMethodsFromCurrentAssembly());
                kojtoCui.SaveAs(kojtoCadCuiFile);
            }
            catch (System.Exception exception)
            {
                Logger.Error(Resources.ErrorCreatingUi, exception);
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(Resources.ErrorCreatingUi);
            }

            LoadUiIntoAutoCad(kojtoCadCuiFile);
            //this.LoadKojtoCAD3DUi();
            //LoadKojtoCad3DUserInterface();
            LoadKojtoCad3DUi();
            
            //EditorHelper.WriteMessage(Resources.SuccessCreatingUi);

        }

        /// <summary>
        /// Regenerate user interface.
        /// </summary>
        [CommandMethod("kojtoMenuRegen")]
        public override void RegenerateUi()
        {
            this.GenerateUi(true);
        }

        public override void RemoveUi()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Load user interface into Autocad.
        /// </summary>
        /// <param name="uiFile">
        /// The user interface file.
        /// </param>
        protected override void LoadUiIntoAutoCad(string uiFile)
        {
            var mainCui = this.GetMainCustomizationSection();
            mainCui.AddPartialMenu(uiFile);
            mainCui.Save();
            Application.ReloadAllMenus();
        }

        /// <summary>
        /// Unload user interface into Autocad.
        /// </summary>
        /// <param name="uiFile">
        /// The user interface file.
        /// </param>
        protected override void UnloadUiFromAutoCad(string uiFile)
        {
            var mainCui = this.GetMainCustomizationSection();
            mainCui.RemovePartialMenu(uiFile, Settings.Default.appName);
            mainCui.Save();
            Application.ReloadAllMenus();
        }

        private void CreateUiFolderIfNotExists(string kojtoCadCuiDir)
        {
            if (!Directory.Exists(kojtoCadCuiDir))
            {
                Directory.CreateDirectory(kojtoCadCuiDir);
            }
        }

        private CustomizationSection BuildMenu(
            XmlDocument kojtoCadMenuDefinition,
            Dictionary<CommandMethodAttribute, MethodInfo> commandsAndMethodsDictionary)
        {
            // rebuild the new cui
            // Create a customization section for our partial menu
            CustomizationSection kojtoCui = new CustomizationSection();
            kojtoCui.MenuGroupName = Settings.Default.appName;

            // add a menu group
            var kojtoCadMacroGroup = new MacroGroup(kojtoCui.MenuGroupName, kojtoCui.MenuGroup);
            foreach (XmlNode toolBarDefinition in kojtoCadMenuDefinition.DocumentElement.ChildNodes)
            {
                var toolBar = new Toolbar(toolBarDefinition.Attributes["name"].Value, kojtoCui.MenuGroup);
                toolBar.ToolbarOrient = ToolbarOrient.right;
                toolBar.ToolbarVisible = ToolbarVisible.show;
                foreach (XmlNode buttonDefinition in toolBarDefinition)
                {
                    foreach (KeyValuePair<CommandMethodAttribute, MethodInfo> commandAndMethod in
                        commandsAndMethodsDictionary)
                    {
                        CommandMethodAttribute commandMethodAttribute = commandAndMethod.Key;
                        MethodInfo method = commandAndMethod.Value;

                        if (method.Name.Length <= "Start".Length)
                        {
                            continue;
                        }
                        string methodName = method.Name.Substring(0, method.Name.Length - "Start".Length);
                        if (!methodName.Equals(buttonDefinition.Attributes["methodName"].Value,
                                      StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        MenuMacro menuMacro = new MenuMacro(
                            kojtoCadMacroGroup,
                            buttonDefinition.InnerText,
                            commandMethodAttribute.GlobalName,
                            method.Name);

                        string smallImage = "";
                        string mediumImage = "";
                        ;
                        try
                        {
                            smallImage = IconsManager.GetIconFile(
                                commandAndMethod.Value.Name.Replace("Start", ""), IconSize.Small);
                            mediumImage = IconsManager.GetIconFile(
                                commandAndMethod.Value.Name.Replace("Start", ""), IconSize.Medium);

                            menuMacro.macro.SmallImage = smallImage;
                            menuMacro.macro.LargeImage = mediumImage;


                            // The var toolBarButton is not assigned to anything because it needs only to be instantiated in order appear in the toolbar
                            var toolBarButton = new ToolbarButton(
                                menuMacro.ElementID, buttonDefinition.InnerText, toolBar, -1);


                        }
                        catch (System.Exception exception)
                        {
                            Logger.Error(Resources.ErrorCreatingToolbarButton, exception);
                        }
                    }
                }
            }
            return kojtoCui;
        }

        private string GetCuiExtensionForCurrentVersion()
        {
            string cuiExtension;
            //17.2 is AutoCAD 2009 and its extension is CUIX
            //17.1 is AutoCAD 2008 and its extension is CUI
            if (Application.Version.Major <= 17 && Application.Version.Minor <= 1)
            {
                cuiExtension = ".cui";
            }
            else
            {
                cuiExtension = ".cuix";
            }
            return cuiExtension;
        }

        private bool CheckAutoCadVersion(int minimumMajorVersion, int minimumMinorVersion)
        {
            bool autoCadCheckPassed;
            if (Application.Version.Major < minimumMajorVersion && Application.Version.Minor < minimumMinorVersion)
            // If the ver. num. is greater than AutoCAD 2007
            {
                autoCadCheckPassed = false;
            }
            else
            {
                autoCadCheckPassed = true;
            }
            return autoCadCheckPassed;

        }

        private CustomizationSection GetMainCustomizationSection()
        {
            string mainMenuCuiFile;
            //17.2 is AutoCAD 2009 and its extension is CUIX
            //17.1 is AutoCAD 2008 and its extension is CUI
            if (Application.Version.Major <= 17 && Application.Version.Minor <= 1)
            {
                mainMenuCuiFile = Application.GetSystemVariable("MENUNAME") + ".cui";
            }
            else
            {
                mainMenuCuiFile = Application.GetSystemVariable("MENUNAME") + ".cuix";
            }
            var mainCui = new CustomizationSection(mainMenuCuiFile);
            return mainCui;
        }

        protected override bool UiExistsInAutoCAD(string uiFile)
        {
            if (GetMainCustomizationSection().PartialCuiFiles.Contains(uiFile))
            {
                return true;
            }
            return false;
        }

        public void LoadKojtoCad3DUi()
        {
            var mainCui = this.GetMainCustomizationSection();
            mainCui.AddPartialMenu(DirectoriesAndFiles.KCad3DCuixFile);
            mainCui.Save();
            Application.ReloadAllMenus(); 
        }
    }
}
#endif