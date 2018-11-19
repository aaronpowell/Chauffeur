using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public UpgradeDeliverable(
            TextReader reader,
            TextWriter writer,
            IMigrationRunnerService migrationRunner,
            IMigrationEntryService migrationEntryService
            )
            : base(reader, writer)
        {
            this.migrationRunner = migrationRunner;
            this.migrationEntryService = migrationEntryService;
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
