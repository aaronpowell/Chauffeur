using System.Linq;
using Chauffeur.Deliverables;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace Chauffeur.Tests.Deliverables
{
    [TestFixture]
    class ChangeAliasDeliverableTests
    {
        [Test]
        public async Task NoArguments_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var delivery = new ChangeAliasDeliverable(writer, null, null);

            await delivery.Run(null, new string[0]);

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task WhatOnly_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var delivery = new ChangeAliasDeliverable(writer, null, null);

            await delivery.Run(null, new[] { "dt" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task MissingNewAlias_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var delivery = new ChangeAliasDeliverable(writer, null, null);

            await delivery.Run(null, new[] { "dt", "old" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
        }

        [TestCase("document-type")]
        [TestCase("doc-type")]
        [TestCase("dt")]
        public async Task ProvidingValidDocTypeAlias_WillUpdateToNewAlias(string what)
        {
            var writer = new MockTextWriter();

            var cts = Substitute.For<IContentTypeService>();
            const string old = "old";
            const string @new = "new";
            var result = Substitute.For<IContentType>();
            result.Alias = old;
            cts.GetContentType(Arg.Is(old)).Returns(result);

            var deliverable = new ChangeAliasDeliverable(writer, null, cts);

            await deliverable.Run(null, new[] { what, old, @new });

            Assert.That(result.Alias, Is.EqualTo(@new));
        }
    }
}
