using System;
using System.Linq;
using Chauffeur.Deliverables;
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
            var writer = Writer();
            
            var deliverable = new UpgradeDeliverable(null, writer, null, MigrationEntryService());

            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task SameVersions_NothingDone()
        {
            var writer = Writer();
            
            var deliverable = new UpgradeDeliverable(null, writer, null, MigrationEntryService(UmbracoVersion.GetSemanticVersion()));

            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task CorrectSameVersionSelected_NothingDone()
        {
            var writer = Writer();
            
            var deliverable = new UpgradeDeliverable(null, writer, null, MigrationEntryService(new SemVersion(7, 1), UmbracoVersion.GetSemanticVersion(), new SemVersion(7,3)));

            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task DifferentVersions_UpgradeIsCompleted()
        {
            var writer = Writer();
            var deliverable = new UpgradeDeliverable(null, writer, Upgrader(true), MigrationEntryService(new SemVersion(7,1)));

            var response = await deliverable.Run(null, null);

            Assert.True(writer.Messages.Count() == 2);
            Assert.Equal(DeliverableResponse.Continue, response);
        }

        [Fact]
        public async Task UpgradeFailes_ErrorStateReturned()
        {
            var writer = Writer();
            var deliverable = new UpgradeDeliverable(null, writer, Upgrader(false), MigrationEntryService(new SemVersion(7, 1)));

            var response = await deliverable.Run(null, null);

            Assert.True(writer.Messages.Count() == 2);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        private MockTextWriter Writer()
        {
            return new MockTextWriter();
        }
        
        private IMigrationEntryService MigrationEntryService(params SemVersion[] versions)
        {
            var migrationEntryService = Substitute.For<IMigrationEntryService>();
            var entries = versions.Select(v => new MigrationEntry(1, DateTime.Now, Constants.System.UmbracoMigrationName, v));
            migrationEntryService.GetAll(Constants.System.UmbracoMigrationName).Returns(entries);
            return migrationEntryService;
        }

        private IUpgrader Upgrader(bool returns)
        {
            var upgrader = Substitute.For<IUpgrader>();
            upgrader.Upgrade(new SemVersion(7, 1), UmbracoVersion.GetSemanticVersion()).Returns(returns);
            return upgrader;
        }
    }
}
