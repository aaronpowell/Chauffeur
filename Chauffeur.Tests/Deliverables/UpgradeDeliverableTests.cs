using Chauffeur.Deliverables;
using NSubstitute;
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
            var migrationEntryService = Substitute.For<IMigrationEntryService>();

            var deliverable = new UpgradeDeliverable(null, writer, null, migrationEntryService);

            var response = await deliverable.Run(null, null);

            Assert.Single(writer.Messages);
            Assert.Equal(DeliverableResponse.FinishedWithError, response);
        }
    }
}
