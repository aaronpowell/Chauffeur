using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
namespace Chauffeur.DependencyBuilders
{
    class MediaServiceBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<IMediaService>(() => 
                new MediaService(container.Resolve<RepositoryFactory>())
            );
        }
    }
}
