using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Chauffeur.DependencyBuilders
{
    class SqlSyntaxProviderBuilder : IBuildDependencies
    {
        public void Build(IContainer container)
        {
            container.Register<Func<string, ISqlCeEngine>>(() => s => new SqlCeEngineWrapper(s));
        }
    }

    public interface ISqlCeEngine
    {
        void CreateDatabase();
    }

    internal class SqlCeEngineWrapper : ISqlCeEngine
    {
        private readonly SqlCeEngine engine;
        public SqlCeEngineWrapper(string connectionString)
        {
            engine = new SqlCeEngine(connectionString);
        }
        public void CreateDatabase()
        {
            engine.CreateDatabase();
        }
    }
}
