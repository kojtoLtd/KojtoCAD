#if bcad
using BricscadApp;
using BricscadDb;
using KojtoCAD.Ui.Interfaces;
using KojtoCAD.Utilities;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Application = Bricscad.ApplicationServices.Application;


namespace KojtoCAD.KojtoCAD3D.MenuInterface
{
    public class BcadMenuGenerator
    {
        private readonly IMenuSchemaProvider _menuSchemaProvider;

        public BcadMenuGenerator(IMenuSchemaProvider menuSchemaProvider)
        {
            _menuSchemaProvider = menuSchemaProvider;
        }

        public void GenerateUiFile()
        {
            var cuiFile = DirectoriesAndFiles.Bcad3DCuiFile;
            var cuiFileName = new FileInfo(cuiFile).Name.Replace(new FileInfo(cuiFile).Extension, string.Empty);

            var app = Application.AcadApplication as AcadApplication;
            if (app == null)
            {
                MessageBox.Show("App was null!", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var menuGroups = app.MenuGroups;
            var myGroup = menuGroups.GetMenuGroupByName(cuiFileName) ?? menuGroups.Load(cuiFile);

            var kojto3DMenu = myGroup.Menus.GetPopUpMenuGroupByName(cuiFileName) ?? myGroup.Menus.Add(cuiFileName);
            var menuSchema = _menuSchemaProvider.GetKojto3DMenuSchema();
            if (menuSchema.DocumentElement != null)
            {
                Dfs(menuSchema.DocumentElement.ChildNodes, kojto3DMenu);
            }
            myGroup.Save(AcMenuFileType.acMenuFileSource);
        }

        private void Dfs(XmlNodeList nodes, AcadPopupMenu parentMenu)
        {
            var position = 1;
            foreach (XmlNode node in nodes)
            {
                position++;
                var typeAttribute = node.Attributes["type"];
                if (typeAttribute == null)
                {
                    MessageBox.Show("MenuItemType was null", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }
                MenuItemType type;
                var availableType = MenuItemType.TryParse(typeAttribute.Value, out type);
                if (!availableType)
                {
                    MessageBox.Show("Unrecognized menu item type", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }
                switch (type)
                {
                    case MenuItemType.MenuItem:
                        var label = node.Attributes["label"].Value;
                        var macro = node.Attributes["macro"].Value;
                        parentMenu.AddMenuItem(position, label, macro);
                        break;
                    case MenuItemType.Separator:
                        parentMenu.AddSeparator(position);
                        break;
                    case MenuItemType.SubMenu:
                        label = node.Attributes["label"].Value;
                        var menu = parentMenu.AddSubMenu(position, label);
                        Dfs(node.ChildNodes, menu);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public enum MenuItemType
    {
        MenuItem,
        Separator,
        SubMenu
    }
}

#endif
