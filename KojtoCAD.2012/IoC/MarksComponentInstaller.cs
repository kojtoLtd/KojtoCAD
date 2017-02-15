using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;

namespace KojtoCAD.IoC
{
    public class MarksComponentInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBlockDrawingProvider>().ImplementedBy<DefaultBlockProvider>());
        }
    }
}
