using System;
using System.IO;
using System.Threading.Tasks;
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

        public const string TableName = "Chauffeur_Delivery";

        public DeliveryDeliverable(
            TextReader reader,
            TextWriter writer,
            UmbracoDatabase database
        ) : base(reader, writer)
        {
            this.database = database;
        }

        public override async System.Threading.Tasks.Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!database.TableExist(TableName))
            {
                if (!await SetupDatabase())
                {
                    return DeliverableResponse.Continue;
                }
            }

            return DeliverableResponse.Continue;
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
