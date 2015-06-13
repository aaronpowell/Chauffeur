using Umbraco.Core.Persistence.UnitOfWork;

namespace Chauffeur.DependencyBuilders
{
    class DatabaseUnitOfWorkProviderBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register<PetaPocoUnitOfWorkProvider, IDatabaseUnitOfWorkProvider>();
        }
    }
}
