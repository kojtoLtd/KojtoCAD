using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using KojtoCAD.Ui;
using KojtoCAD.Ui.Interfaces;

namespace KojtoCAD.IoC
{
    public class UiComponentsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
#if !bcad
            container.Register( Component.For<IIconManager>().ImplementedBy<DefaultIconManager>(),
                                Component.For<IMenuSchemaProvider>().ImplementedBy<DefaultMenuSchemaProvider>(),
                                Component.For<IUiGenerator>().ImplementedBy<ClassicUiGenerator>()
                                
            );
#else
            container.Register(Component.For<IIconManager>().ImplementedBy<DefaultIconManager>(),
                Component.For<IMenuSchemaProvider>().ImplementedBy<DefaultMenuSchemaProvider>(),
                Component.For<IUiGenerator>().ImplementedBy<BcadUiGenerator>()

                );
#endif
        }
    }
}