using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class SettingsDeliverableTests
    {
        [Fact]
        public async Task WillAlwaysGetAFullTable()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            settings.ConnectionString.Returns(new System.Configuration.ConnectionStringSettings("umbracoDbDNS", ""));

            var deliverable = new SettingsDeliverable(null, writer, settings);

            await deliverable.Run(null, null);

            Assert.Equal(7, writer.Messages.Count());
        }

        [Fact]
        public async Task CanGetSiteRootPath()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            const string expected = "c:\\foo";
            settings.TryGetSiteRootDirectory(out string ignore).Returns(x =>
            {
                x[0] = expected;
                return true;
            });
            settings.ConnectionString.Returns(new System.Configuration.ConnectionStringSettings("umbracoDbDNS", ""));

            var deliverable = new SettingsDeliverable(null, writer, settings);

            await deliverable.Run(null, null);

            var actual = writer.Messages.Skip(3).First().Split('|')[1].Trim();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task CanGetUmbracoPath()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            const string expected = "c:\\foo";
            settings.TryGetUmbracoDirectory(out string ignore).Returns(x =>
            {
                x[0] = expected;
                return true;
            });
            settings.ConnectionString.Returns(new System.Configuration.ConnectionStringSettings("umbracoDbDNS", ""));

            var deliverable = new SettingsDeliverable(null, writer, settings);

            await deliverable.Run(null, null);

            var actual = writer.Messages.Skip(4).First().Split('|')[1].Trim();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task CanGetChauffeurPath()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            const string expected = "c:\\foo";
            settings.TryGetChauffeurDirectory(out string ignore).Returns(x =>
            {
                x[0] = expected;
                return true;
            });
            settings.ConnectionString.Returns(new System.Configuration.ConnectionStringSettings("umbracoDbDNS", ""));

            var deliverable = new SettingsDeliverable(null, writer, settings);

            await deliverable.Run(null, null);

            var actual = writer.Messages.Skip(5).First().Split('|')[1].Trim();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task CanGetConnectionString()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            const string expected = "Data Source=|DataDirectory|\\Umbraco.sdf;Flush Interval=1;";
            settings.ConnectionString.Returns(new System.Configuration.ConnectionStringSettings("umbracoDbDNS", expected));

            var deliverable = new SettingsDeliverable(null, writer, settings);

            await deliverable.Run(null, null);

            var actual = string.Join("|", writer.Messages.Skip(6).First().Split('|').Skip(1)).Trim();

            Assert.Equal(expected, actual);
        }
    }
}
