using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace KojtoCAD.IoC
{
    public class UpdaterComponentsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromThisAssembly().InNamespace("KojtoCAD.Updater", true).WithService.DefaultInterfaces());
        }
    }
}
