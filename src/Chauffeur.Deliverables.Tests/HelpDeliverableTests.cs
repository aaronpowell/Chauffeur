using Chauffeur.Components;
using Chauffeur.Deliverables;
using NSubstitute;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Xunit;

namespace Chauffeur.Deliverables.Tests
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

            public async Task<bool> Directions()
            {
                await Out.WriteLineAsync("I have directions");
                return true;
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
            var container = Substitute.For<IFactory>();

            var writer = new MockTextWriter();
            container.GetAllInstances<Deliverable>()
                .Returns(new[] { new MockDeliverable(writer) });

            var deliverable = new HelpDeliverable(null, writer, container);

            await deliverable.Run(null, new string[0]);

            Assert.Equal(2, writer.Messages.Count());
            Assert.Equal("mock (aliases: m)", writer.Messages.Last());
        }

        [Fact]
        public async Task DisplaysHelpForSingleDeliverable()
        {
            var container = Substitute.For<IFactory>();

            var writer = new MockTextWriter();
            var mockDeliverable = new MockDeliverable(writer);
            var resolver = new DeliverableResolver(
                container,
                new[] { new Registration(mockDeliverable.GetType(), new[] { "mock" }) }
            );
            container.GetInstance<DeliverableResolver>().Returns(resolver);
            container.GetInstance<MockDeliverable>().Returns(mockDeliverable);

            var deliverable = new HelpDeliverable(null, writer, container);

            await deliverable.Run(null, new[] { "mock" });

            Assert.Equal("I have directions", writer.Messages.First());
        }

        [Fact]
        public async Task DisplaysErrorWhenDeliverableDoesntProvideDirections()
        {
            var container = Substitute.For<IFactory>();

            var writer = new MockTextWriter();
            var mockDeliverable = new MockDeliverableNoDirections(writer);
            var resolver = new DeliverableResolver(
                container,
                new[] { new Registration(mockDeliverable.GetType(), new[] { "mock2" }) }
            );
            container.GetInstance<DeliverableResolver>().Returns(resolver);
            container.GetInstance<MockDeliverableNoDirections>().Returns(mockDeliverable);

            var deliverable = new HelpDeliverable(null, writer, container);

            await deliverable.Run(null, new[] { "mock2" });

            Assert.Equal("The deliverable 'mock2' doesn't implement help, you best contact the author", writer.Messages.First());
        }

        [Fact]
        public async Task HelpProvidesDirections()
        {
            var container = Substitute.For<IFactory>();
            var writer = new MockTextWriter();
            var deliverable = new HelpDeliverable(null, writer, container);
            var resolver = new DeliverableResolver(
                container,
                new[] { new Registration(deliverable.GetType(), new[] { "help" }) }
            );
            container.GetInstance<DeliverableResolver>().Returns(resolver);
            container.GetInstance<HelpDeliverable>().Returns(deliverable);

            await deliverable.Run(null, new[] { "help" });
            Assert.Equal(4, writer.Messages.Count());
        }
    }
}
