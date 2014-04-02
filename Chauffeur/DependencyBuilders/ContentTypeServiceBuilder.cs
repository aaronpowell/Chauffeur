using Umbraco.Core.Services;

namespace Chauffeur.DependencyBuilders
{
    class ContentTypeServiceBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<ContentTypeService, IContentTypeService>();
        }
    }
}
