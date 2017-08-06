using Chauffeur.Deliverables;
using System.Threading.Tasks;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class QuitDeliverableTests
    {
        [Fact]
        public async Task ReturnsShutdownStatus()
        {
            var deliverable = new QuitDeliverable(null, new MockTextWriter());

            var response = await deliverable.Run("quit", new string[0]);

            Assert.Equal(DeliverableResponse.Shutdown, response);
        }
    }
}
