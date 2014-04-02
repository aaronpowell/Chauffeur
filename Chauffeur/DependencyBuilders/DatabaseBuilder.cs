using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Chauffeur.DependencyBuilders
{
    class DatabaseBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<DatabaseFactory, IDatabaseFactory>();
            container.Register<DatabaseContext, DatabaseContext>();
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
