using System;
using System.Linq;
using Chauffeur.Deliverables;
using Chauffeur.Host;
using Chauffeur.Services;
using NSubstitute;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Chauffeur.Tests.Deliverables
{
    public class UpgradeDeliverableTests
    {
        [Fact]
        public async Task NoMigrationEntries_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            var deliverable = new UpgradeDeliverable(null, writer, null, MigrationEntryService(), settings);

            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task SameVersions_NothingDone()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            var deliverable = new UpgradeDeliverable(null, writer, null, MigrationEntryService(UmbracoVersion.GetSemanticVersion()), settings);

            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task CorrectSameVersionSelected_NothingDone()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            var deliverable = new UpgradeDeliverable(null, writer, null, MigrationEntryService(new SemVersion(7, 1), UmbracoVersion.GetSemanticVersion(), new SemVersion(7,3)), settings);

            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task DifferentVersions_UpgradeIsCompleted()
        {
            var writer = new MockTextWriter();

            var migrationRunnerService = Substitute.For<IMigrationRunnerService>();
            migrationRunnerService.Execute(Arg.Any<SemVersion>(), Arg.Any<SemVersion>()).Returns(true);
            var settings = Substitute.For<IChauffeurSettings>();
            var deliverable = new UpgradeDeliverable(null, writer, migrationRunnerService, MigrationEntryService(new SemVersion(7,1)), settings);

            var response = await deliverable.Run(null, null);

            Assert.Equal(2, writer.Messages.Count());
            Assert.Equal(DeliverableResponse.Continue, response);
        }

        [Fact]
        public async Task UpgradeFailes_ErrorStateReturned()
        {
            var writer = new MockTextWriter();
            var migrationRunnerService = Substitute.For<IMigrationRunnerService>();
            migrationRunnerService.Execute(Arg.Any<SemVersion>(), Arg.Any<SemVersion>()).Returns(false);
            var settings = Substitute.For<IChauffeurSettings>();
            var deliverable = new UpgradeDeliverable(null, writer, migrationRunnerService, MigrationEntryService(new SemVersion(7, 1)), settings);

            var response = await deliverable.Run(null, null);

            Assert.Equal(2, writer.Messages.Count());
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        private IMigrationEntryService MigrationEntryService(params SemVersion[] versions)
        {
            var migrationEntryService = Substitute.For<IMigrationEntryService>();
            var entries = versions.Select(v => new MigrationEntry(1, DateTime.Now, Constants.System.UmbracoMigrationName, v));
            migrationEntryService.GetAll(Constants.System.UmbracoMigrationName).Returns(entries);
            return migrationEntryService;
        }

    }
}
