using Umbraco.Core.Persistence;
namespace Chauffeur.DependencyBuilders
{
    class RepositoryFactoryBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register(() => new RepositoryFactory(true));
        }
    }
}
