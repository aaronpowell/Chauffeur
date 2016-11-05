using System.Linq;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    class InstallDeliverableTests
    {
        [Fact]
        public async Task MissingConnectionString_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            var deliverable = new InstallDeliverable(null, writer, null, settings, null, null);

            await deliverable.Run(null, null);

            Assert.Equal(writer.Messages.Count(), 1);
        }
    }
}
