using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Chauffeur.DependencyBuilders
{
    class ApplicationContextBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<CacheHelper>(CacheHelper.CreateDisabledCacheHelper);
            container.Register<ServiceContext>(() => 
                new ServiceContext(
                    container.Resolve<IContentService>(),
                    container.Resolve<IMediaService>(),
                    container.Resolve<IContentTypeService>(),
                    container.Resolve<IDataTypeService>(),
                    null,
                    null,
                    container.Resolve<IPackagingService>(),
                    null,
                    null,
                    null,
                    null,
                    null
                )
            );

            container.Register<ApplicationContext>(() => 
                new ApplicationContext(
                    container.Resolve<DatabaseContext>(),
                    container.Resolve<ServiceContext>(),
                    container.Resolve<CacheHelper>()
                )
            );

            ApplicationContext.EnsureContext(container.Resolve<ApplicationContext>(), true);
        }
    }
}
