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
    public class PackageDeliverableTests
    {
        [Test]
        public async Task NoPackagesAbortsEarly()
        {
            var writer = Substitute.ForPartsOf<TextWriter>();
            var package = new PackageDeliverable(null, writer, null, null);

            await package.Run(null, new string[0]);

            writer.Received(1).WriteLineAsync(Arg.Any<string>());
        }

        [Test]
        public async Task NotFoundPackageAbortsEarly()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x => {
                x[0] = "";
                return true;
            });
            var package = new PackageDeliverable(null, writer, new MockFileSystem(), settings);

            await package.Run(null, new[] { "Test" });

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
