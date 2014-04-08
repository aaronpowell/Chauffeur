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
            container.Register<ServiceContext>();

            container.Register<ApplicationContext>();

            ApplicationContext.EnsureContext(container.Resolve<ApplicationContext>(), true);
        }
    }
}
