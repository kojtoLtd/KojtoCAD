using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using Microsoft.Win32;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Application = Bricscad.ApplicationServices.Application;
#endif
using Module = System.Reflection.Module;
using RegistryKey = Microsoft.Win32.RegistryKey;
using Registry = Microsoft.Win32.Registry;

[assembly: CommandClass(typeof(KojtoCAD.DemandLoader.DemandLoadingRegistrar))]
namespace KojtoCAD.DemandLoader
{
    public class DemandLoadingRegistrar
    {
        private static readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        /// <summary>
        /// Register KojtoCAD for domand load
        /// </summary>
        [CommandMethod("REGISTERKOJTOCADFORDEMANDLOAD")]
        public static void RegisterForDemandLoading()
        {
            // Get the assembly, its name and location

            Assembly assem = Assembly.GetExecutingAssembly();
            string name = assem.GetName().Name;
            string path = assem.Location;

            // We'll collect information on the commands
            // (we could have used a map or a more complex
            // container for the global and localized names
            // - the assumption is we will have an equal
            // number of each with possibly fewer groups)

            List<string> globCmds = new List<string>();
            List<string> locCmds = new List<string>();
            List<string> groups = new List<string>();

            // Iterate through the modules in the assembly
            Module[] mods = assem.GetModules(true);
            foreach (Module mod in mods)
            {
                // Within each module, iterate through the types

                Type[] types = mod.GetTypes();
                foreach (Type type in types)
                {
                    // We may need to get a type's resources

                    ResourceManager rm = new ResourceManager(type.FullName, assem);
                    rm.IgnoreCase = true;

                    // Get each method on a type

                    MethodInfo[] meths = type.GetMethods();
                    foreach (MethodInfo meth in meths)
                    {
                        // Get the methods custom command attribute(s)

                        object[] attbs = meth.GetCustomAttributes(typeof(CommandMethodAttribute), true);
                        foreach (object attb in attbs)
                        {
                            CommandMethodAttribute cma = attb as CommandMethodAttribute;
                            if (cma != null)
                            {
                                // And we can finally harvest the information
                                // about each command

                                string globName = cma.GlobalName;
                                string locName = globName;
                                string lid = cma.LocalizedNameId;

                                // If we have a localized command ID,
                                // let's look it up in our resources
                                if (lid != null)
                                {
                                    // Let's put a try-catch block around this
                                    // Failure just means we use the global
                                    // name twice (the default)

                                    try
                                    {
                                        locName = rm.GetString(lid);
                                    }
                                    catch { }
                                }

                                // Add the information to our data structures

                                globCmds.Add(globName);
                                locCmds.Add(locName);

                                if (cma.GroupName != null && !groups.Contains(cma.GroupName))
                                    groups.Add(cma.GroupName);
                            }
                        }
                    }
                }
            }

            // Let's register the application to load on demand (12)
            // if it contains commands, otherwise we will have it
            // load on AutoCAD startup (2)
            int flags = (globCmds.Count > 0 ? 12 : 2);

            // By default let's create the commands in HKCU
            // (pass false if we want to create in HKLM)
            CreateDemandLoadingEntries(name, path, globCmds, locCmds, groups, flags, true
            );
        }

        /// <summary>
        /// Unregister KojtoCAD for demand load
        /// </summary>
        [CommandMethod("UNREGISTERKOJTOCADFORDEMANDLOAD")]
        public static void UnregisterForDemandLoading()
        {
            RemoveDemandLoadingEntries(true);
        }

        private static void CreateDemandLoadingEntries(
          string name,
          string path,
          List<string> globCmds,
          List<string> locCmds,
          List<string> groups,
          int flags,
          bool currentUser
        )
        {
            // Choose a Registry hive based on the function input
            //RegistryKey hive = (currentUser ? Registry.CurrentUser : Registry.LocalMachine);
            RegistryKey hive = Registry.CurrentUser;

            // Open the main AutoCAD (or vertical) and "Applications" keys
#if acad2012
            RegistryKey ack = hive.OpenSubKey(HostApplicationServices.Current.RegistryProductRootKey, true);
#endif

#if acad2013
            RegistryKey ack = hive.OpenSubKey(HostApplicationServices.Current.UserRegistryProductRootKey, true);
#endif
#if bcad
            RegistryKey ack = hive.OpenSubKey(HostApplicationServices.Current.RegistryProductRootKey, true);
#endif

            using (ack)
            {
                RegistryKey appk = ack.CreateSubKey("Applications");
                using (appk)
                {
                    // Already registered? Just return

                    string[] subKeys = appk.GetSubKeyNames();
                    foreach (string subKey in subKeys)
                    {
                        if (subKey.Equals(name))
                        {
                            return;
                        }
                    }

                    // Create the our application's root key and its values

                    RegistryKey rk = appk.CreateSubKey(name);
                    using (rk)
                    {
                        rk.SetValue("DESCRIPTION", name, RegistryValueKind.String);
                        rk.SetValue("LOADCTRLS", flags, RegistryValueKind.DWord);
                        rk.SetValue("LOADER", path, RegistryValueKind.String);
                        rk.SetValue("MANAGED", 1, RegistryValueKind.DWord);

                        // Create a subkey if there are any commands...
                        if ((globCmds.Count == locCmds.Count) &&
                            globCmds.Count > 0)
                        {
                            RegistryKey ck = rk.CreateSubKey("Commands");
                            using (ck)
                            {
                                for (int i = 0; i < globCmds.Count; i++)
                                    ck.SetValue(
                                      globCmds[i],
                                      locCmds[i],
                                      RegistryValueKind.String
                                    );
                            }
                        }

                        // And the command groups, if there are any

                        if (groups.Count > 0)
                        {
                            RegistryKey gk = rk.CreateSubKey("Groups");
                            using (gk)
                            {
                                foreach (string grpName in groups)
                                    gk.SetValue(
                                      grpName, grpName, RegistryValueKind.String
                                    );
                            }
                        }
                    }
                }
            }
        }

        private static void RemoveDemandLoadingEntries(bool currentUser)
        {
            try
            {
                // Choose a Registry hive based on the function input

                RegistryKey hive = (currentUser ? Registry.CurrentUser : Registry.LocalMachine);

                // Open the main AutoCAD (vertical) and "Applications" keys
#if acad2012
                RegistryKey ack = hive.OpenSubKey(HostApplicationServices.Current.RegistryProductRootKey);
#endif
#if acad2013
                RegistryKey ack = hive.OpenSubKey(HostApplicationServices.Current.UserRegistryProductRootKey, true);
#endif
#if bcad
                RegistryKey ack = hive.OpenSubKey(HostApplicationServices.Current.RegistryProductRootKey);
#endif
                using (ack)
                {
                    RegistryKey appk = ack.OpenSubKey("Applications", true);
                    using (appk)
                    {
                        // Delete the key with the same name as this assembly

                        appk.DeleteSubKeyTree(Assembly.GetExecutingAssembly().GetName().Name);
                    }
                }
            }
            catch { }
        }
    }
}
