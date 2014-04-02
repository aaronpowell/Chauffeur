using System.IO;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using NSubstitute;
using NUnit.Framework;
namespace Chauffeur.Tests.Deliverables
{
    [TestFixture]
    class UnknownDeliverableTests
    {
        [Test]
        public async Task ShouldReturnContinue()
        {
            var deliverable = new UnknownDeliverable(null, Substitute.ForPartsOf<TextWriter>());

            var result = await deliverable.Run("", new[] { "" });

            Assert.That(result, Is.EqualTo(DeliverableResponse.Continue));
        }
    }
}
