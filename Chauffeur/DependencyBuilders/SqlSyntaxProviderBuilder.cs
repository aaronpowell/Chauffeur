using System.Configuration;
using System.Linq;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Persistence.SqlSyntax;
using System;
namespace Chauffeur.DependencyBuilders
{
    class SqlSyntaxProviderBuilder : IBuildDependencies
    {
        public void Build(ShittyIoC container)
        {
            var providerName = ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ProviderName;

            var provider = TypeFinder.FindClassesWithAttribute<SqlSyntaxProviderAttribute>()
                .FirstOrDefault(p => p.GetCustomAttribute<SqlSyntaxProviderAttribute>(false).ProviderName == providerName);

            if (provider == null)
                throw new FileNotFoundException(string.Format("Unable to find SqlSyntaxProvider that is used for the provider type '{0}'", providerName));

            SqlSyntaxContext.SqlSyntaxProvider = (ISqlSyntaxProvider)Activator.CreateInstance(provider);

        }
    }
}
