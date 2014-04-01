using System.IO;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using NSubstitute;
using NUnit.Framework;

namespace Chauffeur.Tests.Deliverables
{
    [TestFixture]
    public class DeliveryDeliverableTests
    {
        [Test]
        public async Task NoDeliveriesAbortsEarly()
        {
            var writer = Substitute.ForPartsOf<TextWriter>();
            var delivery = new DeliveryDeliverable(null, writer);

            await delivery.Run(null, new string[0]);

            writer.Received(1).WriteLineAsync(Arg.Any<string>());
        }
    }
}
