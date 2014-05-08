using System;
using System.Data.Common;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Chauffeur.Host;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax;
namespace Chauffeur.Deliverables
{
    [DeliverableName("delivery")]
    [DeliverableAlias("d")]
    public sealed class DeliveryDeliverable : Deliverable
    {
        private readonly UmbracoDatabase database;
        private readonly IChauffeurSettings settings;
        private readonly IFileSystem fileSystem;
        private readonly IChauffeurHost host;

        public const string TableName = "Chauffeur_Delivery";

        public DeliveryDeliverable(
            TextReader reader,
            TextWriter writer,
            UmbracoDatabase database,
            IChauffeurSettings settings,
            IFileSystem fileSystem,
            IChauffeurHost host
        )
            : base(reader, writer)
        {
            this.database = database;
            this.settings = settings;
            this.fileSystem = fileSystem;
            this.host = host;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            try
            {
                if (!database.TableExist(TableName))
                {
                    if (!await SetupDatabase())
                        return DeliverableResponse.Continue;
                }
            }
            catch (DbException)
            {
                Out.WriteLine("There was an error checking for the database Chauffeur Delivery tracking table, most likely your connection string is invalid or your database doesn't exist.");
                Out.WriteLine("At present a Chauffeur Delivery is not able to run the `install` deliverable, please run that manually");
                return DeliverableResponse.FinsihedWithError;
            }

            string directory;
            if (!settings.TryGetChauffeurDirectory(out directory))
            {
                await Out.WriteLineAsync("Error accessing the Chauffeur directory. Check your file system permissions");
                return DeliverableResponse.Continue;
            }

            var allDeliveries = fileSystem.Directory
                .GetFiles(directory, "*.delivery", SearchOption.TopDirectoryOnly)
                .ToArray();

            if (!allDeliveries.Any())
            {
                await Out.WriteLineAsync("No deliveries found.");
                return DeliverableResponse.Continue;
            }

            await ProcessDeliveries(allDeliveries);

            return DeliverableResponse.Continue;
        }

        private async Task ProcessDeliveries(string[] allDeliveries)
        {
            foreach (var delivery in allDeliveries)
            {
                var file = fileSystem.FileInfo.FromFileName(delivery);

                var name = file.Name;

                var sql = new Sql()
                    .From<ChauffeurDeliveryTable>()
                    .Where<ChauffeurDeliveryTable>(t => t.Name == name);

                var entry = database.Fetch<ChauffeurDeliveryTable>(sql).FirstOrDefault();

                if (entry != null && entry.SignedFor)
                {
                    await Out.WriteLineFormattedAsync("'{0}' is already signed for, skipping it.", name);
                    continue;
                }

                var instructions = fileSystem.File.ReadAllLines(file.FullName).Where(x => !string.IsNullOrEmpty(x));

                var tracking = new ChauffeurDeliveryTable
                {
                    Name = name,
                    ExecutionDate = DateTime.Now,
                    Hash = HashDelivery(file),
                    SignedFor = true
                };
                foreach (var instruction in instructions)
                {
                    var result = await host.Run(new[] { instruction });

                    if (result != DeliverableResponse.Continue)
                    {
                        tracking.SignedFor = false;
                        break;
                    }
                }
                database.Save(tracking);
                if (!tracking.SignedFor)
                    break;
            }
        }

        private async Task<bool> SetupDatabase()
        {
            await Out.WriteLineAsync("Chauffeur Delivery does not have its database setup. Setting up now.");

            try
            {
                database.CreateTable<ChauffeurDeliveryTable>(true);
            }
            catch (Exception ex)
            {
                Out.WriteLine("Error creating the Chauffeur Delivery tracking table.");
                Out.WriteLine(ex.ToString());
                return false;
            }

            await Out.WriteLineAsync("Successfully created database table.");
            return true;
        }

        private static string HashDelivery(FileInfoBase file)
        {
            using (var fs = file.OpenRead())
            using (var bs = new BufferedStream(fs))
            {
                using (var sha1 = new SHA1Managed())
                {
                    var hash = sha1.ComputeHash(bs);
                    var formatted = new StringBuilder(2 * hash.Length);
                    foreach (var b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return formatted.ToString();
                }
            }
        }
    }

    [TableName(DeliveryDeliverable.TableName)]
    [PrimaryKey("Id")]
    class ChauffeurDeliveryTable
    {
        [Column("Id")]
        [PrimaryKeyColumn(Name = "PK_id", IdentitySeed = 1)]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("ExecutionDate")]
        public DateTime ExecutionDate { get; set; }

        [Column("SignedFor")]
        public bool SignedFor { get; set; }

        [Column("Hash")]
        public string Hash { get; set; }
    }
}
