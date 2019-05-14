using NPoco;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Chauffeur.Deliverables
{
    [DeliverableName("delivery-tracking")]
    public class DeliveryTrackingDeliverable : Deliverable
    {
        private readonly IChauffeurSettings settings;
        private readonly IFileSystem fileSystem;
        private readonly IScopeProvider scopeProvider;
        private readonly ILogger logger;

        public DeliveryTrackingDeliverable(
            TextReader reader,
            TextWriter writer,
            IChauffeurSettings settings,
            IFileSystem fileSystem,
            IScopeProvider scopeProvider,
            ILogger logger) : base(reader, writer)
        {
            this.settings = settings;
            this.fileSystem = fileSystem;
            this.scopeProvider = scopeProvider;
            this.logger = logger;
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            var operation = args[0];

            using (var scope = scopeProvider.CreateScope())
            {
                switch (operation)
                {
                    case "signed-for":
                        await DisplaySignedFor(scope);
                        break;

                    case "available":
                        await DisplayAvailableDeliverables();
                        break;

                    case "status":
                        await DisplayStatus(scope, args.Skip(1));
                        break;

                    default:
                        await Out.WriteLineAsync($"The operation `{operation}` is not supported");
                        break;
                } 
            }

            return DeliverableResponse.Continue;
        }

        private async Task DisplayStatus(IScope scope, IEnumerable<string> args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("No deliveries provided to check the status of");
                return;
            }

            var deliveryNames = args.Select(s => s.EndsWith(".delivery") ? s : s + ".delivery");

            var results = await scope.Database.Query<ChauffeurDeliveryTable>()
                .Where(d => deliveryNames.Contains(d.Name))
                .ToListAsync();
            if (results.Count == 0)
                await Out.WriteLineAsync("None of the specified deliveries have been run.");
            else
            {
                await Out.WriteTableAsync(
                    results.Select(d => new
                    {
                        d.Name,
                        Status = d.SignedFor ? "Signed For" : "Unknown",
                        Date = d.ExecutionDate.ToString("yyy-MM-dd hh:ss")
                    }),
                    new Dictionary<string, string>()
                );
            }
        }

        private async Task DisplayAvailableDeliverables()
        {
            if (!settings.TryGetChauffeurDirectory(out string chauffeurDirectory))
            {
                await Out.WriteLineAsync("Error accessing the Chauffeur directory. Check your file system permissions");
                return;
            }

            var allDeliveries = fileSystem.Directory
                .GetFiles(chauffeurDirectory, "*.delivery", SearchOption.TopDirectoryOnly)
                .ToArray();

            if (!allDeliveries.Any())
                await Out.WriteLineAsync("No deliveries found.");
            else
                await Out.WriteTableAsync(allDeliveries.Select(d => new
                {
                    Name = fileSystem.Path.GetFileName(d),
                    Path = fileSystem.Path.GetFullPath(d)
                }), new Dictionary<string, string>());
        }

        private async Task DisplaySignedFor(IScope scope)
        {
            try
            {
                var dbSchemaHelper = new DatabaseSchemaCreator(scope.Database, logger);
                if (!dbSchemaHelper.TableExists(DeliveryDeliverable.TableName))
                {
                    await Out.WriteLineAsync("The target database hasn't had `delivery` run against it as there's no tracking table");
                    return;
                }
            }
            catch (DbException)
            {
                await Out.WriteLineAsync("There was an error checking for the database Chauffeur Delivery tracking table, most likely your connection string is invalid or your database doesn't exist.");
                return;
            }

            var results = scope.Database.Fetch<ChauffeurDeliveryTable>();

            if (results.Count == 0)
                await Out.WriteLineAsync("No deliveries have previously been signed for.");
            else
                await Out.WriteTableAsync(
                    results.Select(t => new
                    {
                        t.Name,
                        Date = t.ExecutionDate.ToString("yyy-MM-dd hh:ss"),
                        t.Hash
                    }),
                    new Dictionary<string, string>()
                );
        }
    }
}
