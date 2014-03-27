using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Chauffeur.DependencyBuilders
{
    class DatabaseBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            container.Register<IDatabaseFactory>(() => new DatabaseFactory());
            container.Register<DatabaseContext>(() => new DatabaseContext(container.Resolve<IDatabaseFactory>()));
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
