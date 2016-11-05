using System.IO;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using NSubstitute;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    class UnknownDeliverableTests
    {
        [Fact]
        public async Task ShouldReturnContinue()
        {
            var deliverable = new UnknownDeliverable(null, Substitute.ForPartsOf<TextWriter>());

            var result = await deliverable.Run("", new[] { "" });

            Assert.Equal(result, DeliverableResponse.Continue);
        }
    }
}
