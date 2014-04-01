using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;

namespace Chauffeur.Tests.Deliverables
{
    [TestFixture]
    class ContentTypeDeliverableTests
    {
        [Test]
        public async Task GetAllCommandGetsAllFromUmbracoApi()
        {
            var service = Substitute.For<IContentTypeService>();
            service.GetAllContentTypes().Returns(_ => Enumerable.Empty<IContentType>());

            var deliverable = new ContentTypeDeliverable(
                null,
                Substitute.ForPartsOf<TextWriter>(),
                service,
                null,
                null
            );

            await deliverable.Run("", new[] { "get-all" });

            service.Received().GetAllContentTypes();
        }

        [Test]
        public async Task GetCommandWithIdReturnsItemFromUmbracoApi()
        {
            var service = Substitute.For<IContentTypeService>();
            service.GetContentType(Arg.Any<int>()).Returns(Substitute.For<IContentType>());

            var deliverable = new ContentTypeDeliverable(
                null,
                Substitute.For<TextWriter>(),
                service,
                null,
                null
            );

            await deliverable.Run("", new[] { "get", "1" });

            service.Received().GetContentType(Arg.Any<int>());
        }

        [Test]
        public async Task GetCommandWithAliasReturnsItemFromUmbracoApi()
        {
            var service = Substitute.For<IContentTypeService>();
            service.GetContentType(Arg.Any<string>()).Returns(Substitute.For<IContentType>());

            var deliverable = new ContentTypeDeliverable(
                null,
                Substitute.For<TextWriter>(),
                service,
                null,
                null
            );

            await deliverable.Run("", new[] { "get", "alias" });

            service.Received().GetContentType(Arg.Any<string>());
        }
    }
}
