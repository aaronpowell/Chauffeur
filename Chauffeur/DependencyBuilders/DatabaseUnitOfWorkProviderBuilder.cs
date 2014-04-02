using Umbraco.Core.Persistence.UnitOfWork;
namespace Chauffeur.DependencyBuilders
{
    class DatabaseUnitOfWorkProviderBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<PetaPocoUnitOfWorkProvider, IDatabaseUnitOfWorkProvider>();
        }
    }
}
