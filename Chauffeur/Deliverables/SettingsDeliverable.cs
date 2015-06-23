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
                await Out.WriteLineFormattedAsync("Site root directory: {0}", path);
            else
                await Out.WriteLineAsync("Unable to locate the site root directory");

            if (settings.TryGetUmbracoDirectory(out path))
                await Out.WriteLineFormattedAsync("Umbraco directory: {0}", path);
            else
                await Out.WriteLineAsync("Unable to locate the Umbraco directory");

            if (settings.TryGetChauffeurDirectory(out path))
                await Out.WriteLineFormattedAsync("Chauffeur directory: {0}", path);
            else
                await Out.WriteLineAsync("Unable to locate the Chauffeur directory");

            await Out.WriteLineFormattedAsync("Connection string: {0}", settings.ConnectionString);

            return DeliverableResponse.Continue;
        }
    }
}
