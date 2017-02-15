using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;

namespace KojtoCAD.IoC
{
    public static class ContainerRegistrar
    {
        private static WindsorContainer _container;


        public static IWindsorContainer Container
        {
            get { return _container; }
        }

        public static void SetupContainer()
        {
            _container = new WindsorContainer();
            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel));
            _container.Install(FromAssembly.This());
        }

        public static void DisposeContainer()
        {
            _container.Dispose();
            _container = null;
        }

    }
}