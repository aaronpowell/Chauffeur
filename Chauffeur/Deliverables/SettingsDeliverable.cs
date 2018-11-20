using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var dic = new Dictionary<string, string>
            {
                { "Umbraco Version", settings.UmbracoVersion },
                { "Chauffeur Version", settings.ChauffeurVersion }
            };

            if (settings.TryGetSiteRootDirectory(out string path))
                dic.Add("Site Root", path);
            else
                dic.Add("Site Root", "Failed to access");

            if (settings.TryGetUmbracoDirectory(out path))
                dic.Add("Umbraco Directory", path);
            else
                dic.Add("Umbraco Directory", "Failed to access");

            if (settings.TryGetChauffeurDirectory(out path))
                dic.Add("Chauffeur Directory", path);
            else
                dic.Add("Chauffeur Directory", "Failed to access");

            dic.Add("Connection String", settings.ConnectionString.ConnectionString);

            await Out.WriteTableAsync(dic.Keys.Select(key => new
            {
                Setting = key,
                Value = dic[key]
            }));

            return DeliverableResponse.Continue;
        }
    }
}
