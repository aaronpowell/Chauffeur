﻿using Chauffeur.Deliverables;
using NSubstitute;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class HelpDeliverableTests
    {
        [DeliverableName("mock")]
        [DeliverableAlias("m")]
        class MockDeliverable : Deliverable, IProvideDirections
        {
            public MockDeliverable(MockTextWriter writer) : base(null, writer)
            {
            }

            public Task Directions()
            {
                return Out.WriteLineAsync("I have directions");
            }
        }

        [DeliverableName("mock2")]
        class MockDeliverableNoDirections : Deliverable
        {
            public MockDeliverableNoDirections(MockTextWriter writer) : base(null, writer)
            {
            }
        }

        [Fact]
        public async Task DisplaysAllAvailableDeliverables()
        {
            var container = Substitute.For<IContainer>();

            var writer = new MockTextWriter();
            container.ResolveAllDeliverables().Returns(new[] { new MockDeliverable(writer) });

            var deliverable = new HelpDeliverable(null, writer, container);

            await deliverable.Run(null, new string[0]);

            Assert.Equal(2, writer.Messages.Count());
            Assert.Equal("mock (aliases: m)", writer.Messages.Last());
        }

        [Fact]
        public async Task DisplaysHelpForSingleDeliverable()
        {
            var container = Substitute.For<IContainer>();

            var writer = new MockTextWriter();
            var mockDeliverable = new MockDeliverable(writer);
            container.ResolveDeliverableByName(Arg.Is("mock")).Returns(mockDeliverable);

            var deliverable = new HelpDeliverable(null, writer, container);

            await deliverable.Run(null, new[] { "mock" });

            Assert.Equal("I have directions", writer.Messages.First());
        }

        [Fact]
        public async Task DisplaysErrorWhenDeliverableDoesntProvideDirections()
        {
            var container = Substitute.For<IContainer>();

            var writer = new MockTextWriter();
            var mockDeliverable = new MockDeliverableNoDirections(writer);
            container.ResolveDeliverableByName(Arg.Is("mock2")).Returns(mockDeliverable);

            var deliverable = new HelpDeliverable(null, writer, container);

            await deliverable.Run(null, new[] { "mock2" });

            Assert.Equal("The deliverable 'mock2' doesn't implement help, you best contact the author", writer.Messages.First());
        }

        [Fact]
        public async Task HelpProvidesDirections()
        {
            var container = Substitute.For<IContainer>();
            var writer = new MockTextWriter();
            var deliverable = new HelpDeliverable(null, writer, container);
            container.ResolveDeliverableByName(Arg.Is("help")).Returns(deliverable);

            await deliverable.Run(null, new[] { "help" });
            Assert.Equal(4, writer.Messages.Count());
        }
    }
}
