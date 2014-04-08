using Umbraco.Core.Persistence;
namespace Chauffeur.DependencyBuilders
{
    class RepositoryFactoryBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<RepositoryFactory>(() => new RepositoryFactory(true));
        }
    }
}
