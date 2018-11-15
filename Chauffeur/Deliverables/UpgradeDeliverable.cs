using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("upgrade")]
    public sealed class UpgradeDeliverable : Deliverable
    {
        private readonly IUpgrader upgrader;
        private readonly IMigrationEntryService migrationEntryService;

        public UpgradeDeliverable(
            TextReader reader,
            TextWriter writer,
            IUpgrader upgrader,
            IMigrationEntryService migrationEntryService
            )
            : base(reader, writer)
        {
            this.upgrader = upgrader;
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
            var upgraded = upgrader.Upgrade(currentVersion, targetVersion);

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

    public interface IUpgrader
    {
        bool Upgrade(SemVersion from, SemVersion to);
    }

    public class Upgrader : IUpgrader
    {
        private ILogger logger;
        private IMigrationEntryService migrationEntryService;
        private UmbracoDatabase database;

        public Upgrader(IMigrationEntryService migrationEntryService, ILogger logger, UmbracoDatabase database)
        {
            this.logger = logger;
            this.migrationEntryService = migrationEntryService;
            this.database = database;
        }

        public bool Upgrade(SemVersion from, SemVersion to)
        {
            var runner = new MigrationRunner(
                migrationEntryService,
                logger,
                from,
                to,
                Constants.System.UmbracoMigrationName);

            return runner.Execute(database, isUpgrade: true);
        }
    }
}
