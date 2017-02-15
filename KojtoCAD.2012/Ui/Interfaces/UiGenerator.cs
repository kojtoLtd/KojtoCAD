using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Core.Logging;
#if !bcad
using Autodesk.AutoCAD.Runtime;
#else
using Teigha.Runtime;
#endif

namespace KojtoCAD.Ui.Interfaces
{
    public abstract class UiGenerator : IUiGenerator
    {
        private ILogger _logger = NullLogger.Instance;
        private IIconManager _iconsManager;
        private IMenuSchemaProvider _menuSchemaProvider;

        protected UiGenerator(IIconManager iconsManager, IMenuSchemaProvider menuSchemaProvider, ILogger logger)
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _iconsManager = iconsManager;
            _menuSchemaProvider = menuSchemaProvider;

        }

        protected UiGenerator()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _iconsManager = IoC.ContainerRegistrar.Container.Resolve<IIconManager>();
            _menuSchemaProvider = IoC.ContainerRegistrar.Container.Resolve<IMenuSchemaProvider>();
        }

        protected ILogger Logger
        {
            get { return _logger; }
        }

        protected IIconManager IconsManager
        {
            get { return _iconsManager; }
        }

        protected IMenuSchemaProvider MenuSchemaProvider
        {
            get { return _menuSchemaProvider; }
        }

        public abstract void GenerateUi(bool regenerateIfExists);
        public abstract void RegenerateUi();
        public abstract void RemoveUi();
        protected abstract void LoadUiIntoAutoCad(string uiFile);
        protected abstract void UnloadUiFromAutoCad(string uiFile);
        protected abstract bool UiExistsInAutoCAD(string uiFile);
        protected Dictionary<CommandMethodAttribute, MethodInfo> GetCommandMethodsFromCurrentAssembly()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetExportedTypes();

            Dictionary<CommandMethodAttribute, MethodInfo> commandsAndMethodsDictionary =
                new Dictionary<CommandMethodAttribute, MethodInfo>();
            foreach (Type type in types)
            {
                foreach (MethodInfo methodInfo in type.GetMethods())
                {
                    if (methodInfo == null)
                    {
                        continue;
                    }
                    if (methodInfo.Name.Contains("NotUsedDirectly"))
                    {
                        continue;
                    }
                    var methodsCustomAttributes = methodInfo.GetCustomAttributes(typeof(CommandMethodAttribute), true);
                    if (methodsCustomAttributes.Length == 0)
                    {
                        continue;
                    }
                    CommandMethodAttribute commandMethodAttribute =
                        methodInfo.GetCustomAttributes(typeof(CommandMethodAttribute), true)[0] as
                        CommandMethodAttribute;
                    if (commandMethodAttribute != null)
                    {
                        commandsAndMethodsDictionary.Add(commandMethodAttribute, methodInfo);
                    }
                }
            }
            return commandsAndMethodsDictionary;
        }
    }
}
