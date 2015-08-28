using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur.DependencyBuilders;
using Chauffeur.Host;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Chauffeur.Deliverables
{
    [DeliverableName("install")]
    public sealed class InstallDeliverable : Deliverable
    {
        private readonly DatabaseContext context;
        private readonly IChauffeurSettings settings;
        private readonly Func<string, ISqlCeEngine> sqlCeEngineFactory;
        private readonly IFileSystem fileSystem;

        public InstallDeliverable(
            TextReader reader,
            TextWriter writer,
            DatabaseContext context,
            IChauffeurSettings settings,
            Func<string, ISqlCeEngine> sqlCeEngineFactory,
            IFileSystem fileSystem
            )
            : base(reader, writer)
        {
            this.context = context;
            this.settings = settings;
            this.sqlCeEngineFactory = sqlCeEngineFactory;
            this.fileSystem = fileSystem;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            var connectionString = settings.ConnectionString;
            if (connectionString == null || string.IsNullOrEmpty(connectionString.ConnectionString))
            {
                await Out.WriteLineAsync("No connection string is setup for your Umbraco instance. Chauffeur expects your web.config to be setup in your deployment package before you try and install.");
                return DeliverableResponse.Continue;
            }

            if (connectionString.ProviderName == "System.Data.SqlServerCe.4.0")
            {
                var dataDirectory = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
                var dataSource = connectionString.ConnectionString.Split(';').FirstOrDefault(s => s.ToLower().Contains("data source"));

                if (!string.IsNullOrEmpty(dataSource))
                {
                    var dbFileName = dataSource.Split('=')
                        .Last()
                        .Split('\\')
                        .Last()
                        .Trim();

                    var location = fileSystem.Path.Combine(dataDirectory, dbFileName);

                    if (!fileSystem.File.Exists(location))
                    {
                        await Out.WriteLineAsync("The SqlCE database specified in the connection string doesn't appear to exist.");
                        var response = args.Length > 0 ? args[0] : null;
                        if (string.IsNullOrEmpty(response))
                        {
                            await Out.WriteAsync("Create it? (Y/n) ");
                            response = await In.ReadLineAsync();
                        }

                        if (string.IsNullOrEmpty(response) || response.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                        {
                            await Out.WriteLineAsync("Creating the database");
                            var engine = sqlCeEngineFactory(connectionString.ConnectionString);
                            engine.CreateDatabase();
                        }
                        else
                        {
                            await Out.WriteLineAsync("Installation is being aborted");
                            return DeliverableResponse.Continue;
                        }
                    }
                }
            }

            await Out.WriteLineAsync("Preparing to install Umbraco's database");

            context.Database.CreateDatabaseSchema(false);

            await Out.WriteLineAsync("Database installed and ready to go");

            return DeliverableResponse.Continue;
        }
    }
}
