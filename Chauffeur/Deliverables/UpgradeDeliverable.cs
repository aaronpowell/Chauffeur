using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;

namespace Chauffeur.Deliverables
{
    [DeliverableName("upgrade")]
    public sealed class UpgradeDeliverable : Deliverable
    {
        private readonly ApplicationContext appContext;
	    private readonly IMigrationEntryService migrationEntryService;

		public UpgradeDeliverable(
            TextReader reader,
            TextWriter writer,
            ApplicationContext appContext,
			IMigrationEntryService migrationEntryService
            )
            : base(reader, writer)
        {
            this.appContext = appContext;
	        this.migrationEntryService = migrationEntryService;

        }

        public override async Task<DeliverableResponse> Run(string command, string[] args)
        {
	        try
	        {
		        var currentVersion = FindCurrentDbVersion();
		        await Out.WriteLineAsync(
			        $"Attempting to upgrade from {currentVersion} to {UmbracoVersion.GetSemanticVersion()}");

		        var runner = new MigrationRunner(
			        migrationEntryService,
			        appContext.ProfilingLogger.Logger, 
			        currentVersion, 
			        UmbracoVersion.GetSemanticVersion(),
			        Constants.System.UmbracoMigrationName);

		        var upgraded = runner.Execute(appContext.DatabaseContext.Database, isUpgrade:true);

		        if (upgraded == false)
		        {
			        await base.Out.WriteLineAsync("Upgrading failed, see log for full details");
			        return DeliverableResponse.FinishedWithError;
		        }

		        await base.Out.WriteLineAsync("Upgrading completed");
		        return DeliverableResponse.Continue;
	        }
	        catch (NoConfiguredVersionException)
	        {
		        await base.Out.WriteLineAsync("Can't upgrade as there is no configured version");
		        return DeliverableResponse.FinishedWithError;
	        }
		}

	    private SemVersion FindCurrentDbVersion()
	    {
		    var entries = migrationEntryService.GetAll(Constants.System.UmbracoMigrationName).ToArray();
		    if (entries.Any())
		    {
			    return entries.OrderBy(e => e.Version).Last().Version;
		    }
			throw new NoConfiguredVersionException();
	    }
    }

	public class NoConfiguredVersionException : Exception
	{

	}
}
