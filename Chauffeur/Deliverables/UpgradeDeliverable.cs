using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml;
using Chauffeur.Host;
using Chauffeur.Services;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("upgrade")]
    public sealed class UpgradeDeliverable : Deliverable
    {
        private readonly IMigrationRunnerService migrationRunner;
        private readonly IMigrationEntryService migrationEntryService;
        private readonly IChauffeurSettings chauffeurSettings;

        public UpgradeDeliverable(
            TextReader reader,
            TextWriter writer,
            IMigrationRunnerService migrationRunner,
            IMigrationEntryService migrationEntryService,
            IChauffeurSettings chauffeurSettings
            )
            : base(reader, writer)
        {
            this.migrationRunner = migrationRunner;
            this.migrationEntryService = migrationEntryService;
            this.chauffeurSettings = chauffeurSettings;
        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!TryFindCurrentDbVersion(out var currentVersion))
            {
                await Out.WriteLineAsync("Can't upgrade as there is no configured version");
                return DeliverableResponse.FinishedWithError;
            }

            var targetVersion = UmbracoVersion.GetSemanticVersion();

            if (currentVersion == targetVersion)
            {
                await Out.WriteLineAsync($"Version is up to date {currentVersion} no work todo");
                return DeliverableResponse.FinishedWithError;
            }

            await Out.WriteLineAsync($"Upgrading from {currentVersion} to {targetVersion}");
            var upgraded = migrationRunner.Execute(currentVersion, targetVersion);

            if (!upgraded)
            {
                await Out.WriteLineAsync("Upgrading failed, see log for full details");
                return DeliverableResponse.FinishedWithError;
            }

            chauffeurSettings.TryGetSiteRootDirectory(out string path);
            var configPath = Path.Combine(path, "web.config");
            XmlDocument xmld = new XmlDocument();
            xmld.Load(configPath);

            XmlElement status = (XmlElement)xmld.SelectSingleNode("/configuration/appSettings/add[@key='umbracoConfigurationStatus']");
            if (status != null)
            {
                status.SetAttribute("value", targetVersion.ToString());
            }
            xmld.Save(configPath);

            await Out.WriteLineAsync("Upgrading completed");
            return DeliverableResponse.Continue;
        }

        private bool TryFindCurrentDbVersion(out SemVersion version)
        {
            var entries = migrationEntryService.GetAll(Constants.System.UmbracoMigrationName).ToArray();
            if (entries.Any())
            {
                version = entries.OrderBy(e => e.Version).Last().Version;
                return true;
            }

            version = null;
            return false;
        }
    }

    
}
