using System;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
namespace Chauffeur.ApiWorkarounds
{
    internal static class ApplicationContextWorkaround
    {
        public static IDisposable Create()
        {
            var ac = new ApplicationContext(
                new DatabaseContext(new DatabaseFactory()),
                new ServiceContext(null, null, null, null, null, null, null, null, null, null, null, null),
                CacheHelper.CreateDisabledCacheHelper()
                );

            ApplicationContext.EnsureContext(ac, true);

            return ac;
        }

        private class DatabaseFactory : IDatabaseFactory
        {
            private readonly UmbracoDatabase _umbracoDatabase;
            public DatabaseFactory()
            {
                _umbracoDatabase = new UmbracoDatabase("umbracoDbDSN");
            }
            public UmbracoDatabase CreateDatabase()
            {
                return _umbracoDatabase;
            }

            public void Dispose()
            {
                _umbracoDatabase.Dispose();
            }
        }
    }
}
