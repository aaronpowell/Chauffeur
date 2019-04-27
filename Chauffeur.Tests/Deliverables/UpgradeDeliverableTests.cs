using System;
using System.Linq;
using System.Xml;
using Chauffeur.Deliverables;
using Chauffeur.Host;
using Chauffeur.Services.Interfaces;
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
        private readonly MockTextWriter writer;
        private readonly IChauffeurSettings settings;
        private readonly IXmlDocumentService xmlDocumentWrapper;
        private readonly IMigrationRunnerService migrationRunnerService;
        
        public UpgradeDeliverableTests()
        {
            writer = new MockTextWriter();
            settings = Substitute.For<IChauffeurSettings>();
            xmlDocumentWrapper = Substitute.For<IXmlDocumentService>();
            migrationRunnerService = Substitute.For<IMigrationRunnerService>();
        }

        [Fact]
        public async Task NoMigrationEntries_WillWarnAndExit()
        {
            var deliverable = new UpgradeDeliverable(null, writer, null, MigrationEntryService(), settings, xmlDocumentWrapper);
            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task SameVersions_NothingDone()
        {

            var deliverable = new UpgradeDeliverable(null, writer, null, MigrationEntryService(UmbracoVersion.GetSemanticVersion()), settings, xmlDocumentWrapper);
            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task CorrectSameVersionSelected_NothingDone()
        {

            var deliverable = new UpgradeDeliverable(null, writer, null, 
                MigrationEntryService(new SemVersion(7, 1), UmbracoVersion.GetSemanticVersion(), new SemVersion(7,3)), settings, xmlDocumentWrapper);

            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }

        [Fact]
        public async Task DifferentVersions_UpgradeIsCompleted()
        {
            migrationRunnerService.Execute(Arg.Any<SemVersion>(), Arg.Any<SemVersion>()).Returns(true);
            var anyStringArg = Arg.Any<string>();
            settings.TryGetSiteRootDirectory(out anyStringArg).Returns( 
                x => {
                    x[0] = @"C:\test\umbraco";
                    return true;
                });

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(
                 "<configuration>" +
                 "<appSettings>" +
                 "<add key=\"umbracoConfigurationStatus\" value=\"1.1.1\" />" +
                 "</appSettings>" +
                 "</configuration>"
                 );

            xmlDocumentWrapper.LoadDocument(Arg.Any<string>()).Returns(xmlDoc);

            var deliverable = new UpgradeDeliverable(null, writer, migrationRunnerService, 
                MigrationEntryService(new SemVersion(7,1)), settings, xmlDocumentWrapper);

            var response = await deliverable.Run(null, null);

            Assert.Equal(2, writer.Messages.Count());
            Assert.Equal(DeliverableResponse.Continue, response);
        }

        [Fact]
        public async Task UpgradeFailes_ErrorStateReturned()
        {
            migrationRunnerService.Execute(Arg.Any<SemVersion>(), Arg.Any<SemVersion>()).Returns(false);

            var deliverable = new UpgradeDeliverable(null, writer, migrationRunnerService, 
                MigrationEntryService(new SemVersion(7, 1)), settings, xmlDocumentWrapper);

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
