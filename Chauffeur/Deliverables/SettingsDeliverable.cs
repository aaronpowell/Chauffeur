using System.IO;
using System.Threading.Tasks;
using Chauffeur.Host;

namespace Chauffeur.Deliverables
{
    [DeliverableName("settings")]
    public sealed class SettingsDeliverable : Deliverable
    {
        private readonly IChauffeurSettings settings;

        public SettingsDeliverable(TextReader reader, TextWriter writer, IChauffeurSettings settings) : base(reader, writer)
        {
            this.settings = settings;
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            string path;

            if (settings.TryGetSiteRootDirectory(out path))
                await Out.WriteLineAsync($"Site root directory: ${path}");
            else
                await Out.WriteLineAsync("Unable to locate the site root directory");

            if (settings.TryGetUmbracoDirectory(out path))
                await Out.WriteLineAsync($"Umbraco directory: ${path}");
            else
                await Out.WriteLineAsync("Unable to locate the Umbraco directory");

            if (settings.TryGetChauffeurDirectory(out path))
                await Out.WriteLineAsync($"Chauffeur directory: ${path}");
            else
                await Out.WriteLineAsync("Unable to locate the Chauffeur directory");

            await Out.WriteLineAsync($"Connection string: ${settings.ConnectionString}");

            return DeliverableResponse.Continue;
        }
    }
}
