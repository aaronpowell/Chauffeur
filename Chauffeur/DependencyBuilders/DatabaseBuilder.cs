using Chauffeur.Host;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Chauffeur.DependencyBuilders
{
    class DatabaseBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<DatabaseFactory, IDatabaseFactory>();
            container.Register<DatabaseContext>();
            container.Register<UmbracoDatabase>(() =>
            {
                var connectionString = container.Resolve<IChauffeurSettings>().ConnectionString;
                return new UmbracoDatabase(connectionString.ConnectionString, connectionString.ProviderName);
            });
        }

        private class DatabaseFactory : IDatabaseFactory
        {
            private readonly UmbracoDatabase umbracoDatabase;
            public DatabaseFactory(UmbracoDatabase umbracoDatabase)
            {
                this.umbracoDatabase = umbracoDatabase;
            }
            public UmbracoDatabase CreateDatabase()
            {
                return umbracoDatabase;
            }

            public void Dispose()
            {
                umbracoDatabase.Dispose();
            }
        }
    }
}
