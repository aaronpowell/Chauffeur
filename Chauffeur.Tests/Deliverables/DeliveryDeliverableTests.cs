using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using Chauffeur.Host;
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
            var delivery = new DeliveryDeliverable(null, writer, null, null);

            await delivery.Run(null, new string[0]);

            writer.Received(1).WriteLineAsync(Arg.Any<string>());
        }

        [Test]
        public async Task NotFoundDeliveryAbortsEarly()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x => {
                x[0] = "";
                return true;
            });
            var delivery = new DeliveryDeliverable(null, writer, new MockFileSystem(), settings);

            await delivery.Run(null, new[] { "Test" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
        }

        private class MockTextWriter : TextWriter
        {
            private readonly List<string> messages;
            public MockTextWriter()
            {
                messages = new List<string>();
            }

            public IEnumerable<string> Messages { get { return messages; } }

            public override System.Text.Encoding Encoding { get { return System.Text.Encoding.Default; } }

            public override async Task WriteLineAsync(string value)
            {
                messages.Add(value);
                await Task.FromResult(value);
            }
        }
    }
}
