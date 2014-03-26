using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Chauffeur.DependencyBuilders
{
    class ContentServiceBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<IContentService>(() => 
                new ContentService(container.Resolve<RepositoryFactory>())
            );
        }
    }
}
