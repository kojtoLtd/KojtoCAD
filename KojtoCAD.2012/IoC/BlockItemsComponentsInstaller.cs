using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.Interfaces;

namespace KojtoCAD.IoC
{
    public class BlockItemsComponentsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IPaperSizeFactory>().ImplementedBy<DefaultPaperSizeFactory>());
        }
    }
}

