﻿using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.Interfaces;

namespace KojtoCAD.IoC
{
    public class UtilitiesComponentsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IFileService>().ImplementedBy<FileService>());
            container.Register(Component.For<ILogger>().ImplementedBy<SmartLogger>().IsDefault(c => true));

            container.Register(Component.For<IUtilityClass>().ImplementedBy<UtilityClass>());
            //container.Register(Component.For<IUtilityClass>().ImplementedBy<UtilityClassDebug>());
        }
    }
}