#if bcad
using System;
using BricscadApp;

namespace KojtoCAD.Utilities
{
    public static class BcadExtensionMethods
    {
        public static AcadMenuGroup GetMenuGroupByName(this AcadMenuGroups menuGroups, string name)
        {
            for (var i = 0; i < menuGroups.Count; i++)
            {
                var g = menuGroups.Item(i);
                if (!g.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                return g;
            }
            return null;
        }

        public static AcadToolbar GetToolbarByName(this AcadToolbars toolbars, string name)
        {
            for (var i = 0; i < toolbars.Count; i++)
            {
                var t = toolbars.Item(i);
                if (!t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                return t;
            }
            return null;
        }

        public static AcadToolbarItem GetToolbarItemByName(this AcadToolbar toolbar, string name)
        {
            for (var i = 0; i < toolbar.Count; i++)
            {
                var t = toolbar.Item(i);
                if (!t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                return t;
            }
            return null;
        }

        public static AcadPopupMenu GetPopUpMenuGroupByName(this AcadPopupMenus popUpMenus, string name)
        {
            for (var i = 0; i < popUpMenus.Count; i++)
            {
                var g = popUpMenus.Item(i);
                if (!g.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                return g;
            }
            return null;
        }
    }
}
#endif