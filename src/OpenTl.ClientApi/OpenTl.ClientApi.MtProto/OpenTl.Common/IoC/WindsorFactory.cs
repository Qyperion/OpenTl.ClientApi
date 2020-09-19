using System.Linq;

namespace OpenTl.Common.IoC
{
    using System.Reflection;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

    public static class WindsorFactory
    {
        public static IWindsorContainer Create(params Assembly[] assemblies)
        {
            var container =  new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());

            var uniqueAssemblies = assemblies.Distinct().ToArray();

            container.RegisterPerAttibutes(uniqueAssemblies);

            return container;
        }
    }
}