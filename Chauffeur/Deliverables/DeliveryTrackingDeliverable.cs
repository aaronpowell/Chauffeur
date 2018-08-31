using Chauffeur.Host;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Chauffeur.Deliverables
{
    [DeliverableName("delivery-tracking")]
    public class DeliveryTrackingDeliverable : Deliverable
    {
        private readonly ISqlSyntaxProvider sqlSyntax;
        private readonly Database database;
        private readonly IChauffeurSettings settings;
        private readonly IFileSystem fileSystem;
        private readonly DatabaseSchemaHelper dbSchemaHelper;

        public DeliveryTrackingDeliverable(
            TextReader reader,
            TextWriter writer,
            ISqlSyntaxProvider sqlSyntax,
            Database database,
            IChauffeurSettings settings,
            IFileSystem fileSystem,
            DatabaseSchemaHelper dbSchemaHelper) : base(reader, writer)
        {
            this.sqlSyntax = sqlSyntax;
            this.database = database;
            this.settings = settings;
            this.fileSystem = fileSystem;
            this.dbSchemaHelper = dbSchemaHelper;
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            var operation = args[0];

            switch (operation)
            {
                case "signed-for":
                    await DisplaySignedFor();
                    break;

                case "available":
                    await DisplayAvailableDeliverables();
                    break;

                case "status":
                    await DisplayStatus(args);
                    break;

                default:
                    await Out.WriteLineFormattedAsync("The operation `{0}` is not supported", operation);
                    break;
            }

            return DeliverableResponse.Continue;
        }

        private async Task DisplayStatus(string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("No deliveries provided to check the status of");
                return;
            }

            var deliveryNames = args.Select(s => s.EndsWith(".delivery") ? s : s + ".delivery");

            var sql = new Sql()
                .From<ChauffeurDeliveryTable>(sqlSyntax)
                .Where<ChauffeurDeliveryTable>(d => deliveryNames.Contains(d.Name), sqlSyntax);

            var results = database.Fetch<ChauffeurDeliveryTable>(sql);
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
                    })
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
                }));
        }

        private async Task DisplaySignedFor()
        {
            try
            {
                if (!dbSchemaHelper.TableExist(DeliveryDeliverable.TableName))
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

            var sql = new Sql()
                .From<ChauffeurDeliveryTable>(sqlSyntax);

            var results = database.Fetch<ChauffeurDeliveryTable>(sql);

            if (results.Count == 0)
                await Out.WriteLineAsync("No deliveries have previously been signed for.");
            else
                await Out.WriteTableAsync(
                    results.Select(t => new
                    {
                        t.Name,
                        Date = t.ExecutionDate.ToString("yyy-MM-dd hh:ss"),
                        t.Hash
                    })
                );
        }
    }
}
