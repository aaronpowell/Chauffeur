using Chauffeur.Services.Interfaces;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;

namespace Chauffeur.Services
{
    public class MigrationRunnerService : IMigrationRunnerService
    {
        private ILogger logger;
        private IMigrationEntryService migrationEntryService;
        private UmbracoDatabase database;

        public MigrationRunnerService(IMigrationEntryService migrationEntryService, ILogger logger, UmbracoDatabase database)
        {
            this.logger = logger;
            this.migrationEntryService = migrationEntryService;
            this.database = database;
        }

        public bool Execute(SemVersion from, SemVersion to)
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
