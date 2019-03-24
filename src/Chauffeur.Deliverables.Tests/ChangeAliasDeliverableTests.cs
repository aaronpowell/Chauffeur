using NSubstitute;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace Chauffeur.Deliverables.Tests
{
    public class ChangeAliasDeliverableTests
    {
        [Fact]
        public async Task NoArguments_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var delivery = new ChangeAliasDeliverable(writer, null, null);

            await delivery.Run(null, new string[0]);

            Assert.Single(writer.Messages);
        }

        [Fact]
        public async Task WhatOnly_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var delivery = new ChangeAliasDeliverable(writer, null, null);

            await delivery.Run(null, new[] { "dt" });

            Assert.Single(writer.Messages);
        }

        [Fact]
        public async Task MissingNewAlias_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var delivery = new ChangeAliasDeliverable(writer, null, null);

            await delivery.Run(null, new[] { "dt", "old" });

            Assert.Single(writer.Messages);
        }

        [Theory]
        [InlineData("document-type")]
        [InlineData("doc-type")]
        [InlineData("dt")]
        public async Task ProvidingValidDocTypeAlias_WillUpdateToNewAlias(string what)
        {
            var writer = new MockTextWriter();

            var cts = Substitute.For<IContentTypeService>();
            const string old = "old";
            const string @new = "new";
            var result = Substitute.For<IContentType>();
            result.Alias = old;
            cts.Get(Arg.Is(old)).Returns(result);

            var deliverable = new ChangeAliasDeliverable(writer, null, cts);

            await deliverable.Run(null, new[] { what, old, @new });

            Assert.Equal(result.Alias, @new);
        }
    }
}