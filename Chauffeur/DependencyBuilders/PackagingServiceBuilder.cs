using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
namespace Chauffeur.DependencyBuilders
{
    class PackagingServiceBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<IPackagingService>(() => new PackagingService(
                    container.Resolve<IContentService>(),
                    container.Resolve<IContentTypeService>(),
                    container.Resolve<IMediaService>(),
                    null,
                    container.Resolve<IDataTypeService>(),
                    null,
                    null,
                    container.Resolve<RepositoryFactory>(),
                    container.Resolve<IDatabaseUnitOfWorkProvider>()
                )
            );
        }
    }
}
