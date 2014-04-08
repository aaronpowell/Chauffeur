using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        public InstallDeliverable(
            TextReader reader,
            TextWriter writer,
            DatabaseContext context,
            IChauffeurSettings settings
            )
            : base(reader, writer)
        {
            this.context = context;
            this.settings = settings;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            var connectionString = settings.ConnectionString;
            if (connectionString == null || string.IsNullOrEmpty(connectionString.ConnectionString))
            {
                await Out.WriteLineAsync("No connection string is setup for your Umbraco instance. Chauffeur expects your web.config to be setup in your deployment package before you try and install.");
                return DeliverableResponse.Continue;
            }

            await Out.WriteLineAsync("Preparing to install Umbraco's database");

            context.Database.CreateDatabaseSchema(false);

            await Out.WriteLineAsync("Database installed and ready to go");

            return DeliverableResponse.Continue;
        }
    }
}
