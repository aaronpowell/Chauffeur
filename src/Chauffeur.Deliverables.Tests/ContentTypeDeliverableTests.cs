using System.IO;
using System.Linq;
using NSubstitute;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace Chauffeur.Deliverables.Tests
{
    public class ContentTypeDeliverableTests
    {
        [Fact]
        public async Task GetAllCommandGetsAllFromUmbracoApi()
        {
            var service = Substitute.For<IContentTypeService>();
            service.GetAll().Returns(_ => Enumerable.Empty<IContentType>());

            var deliverable = new ContentTypeDeliverable(
                null,
                Substitute.ForPartsOf<TextWriter>(),
                service,
                null,
                null,
                null
            );

            await deliverable.Run("", new[] { "get-all" });

            service.Received().GetAll();
        }

        [Fact]
        public async Task GetCommandWithIdReturnsItemFromUmbracoApi()
        {
            var service = Substitute.For<IContentTypeService>();
            service.Get(Arg.Any<int>()).Returns(Substitute.For<IContentType>());

            var deliverable = new ContentTypeDeliverable(
                null,
                Substitute.For<TextWriter>(),
                service,
                null,
                null,
                null
            );

            await deliverable.Run("", new[] { "get", "1" });

            service.Received().Get(Arg.Any<int>());
        }

        [Fact]
        public async Task GetCommandWithAliasReturnsItemFromUmbracoApi()
        {
            var service = Substitute.For<IContentTypeService>();
            service.Get(Arg.Any<string>()).Returns(Substitute.For<IContentType>());

            var deliverable = new ContentTypeDeliverable(
                null,
                Substitute.For<TextWriter>(),
                service,
                null,
                null,
                null
            );

            await deliverable.Run("", new[] { "get", "alias" });

            service.Received().Get(Arg.Any<string>());
        }

        [Fact]
        public async Task RemoveCommandWillCallRemoveMethod()
        {
            var service = Substitute.For<IContentTypeService>();
            service.Get(Arg.Any<string>()).Returns(Substitute.For<IContentType>());
            service.Delete(Arg.Any<IContentType>());

            var deliverable = new ContentTypeDeliverable(
                null,
                Substitute.For<TextWriter>(),
                service,
                null,
                null,
                null
            );

            await deliverable.Run("", new[] { "remove", "alias" });

            service.Received().Delete(Arg.Any<IContentType>());
        }

        [Fact]
        public async Task RemovePropertyCommandWillRemoveAndSaveContentType()
        {
            var service = Substitute.For<IContentTypeService>();
            var ct = Substitute.For<IContentType>();

            service.Get(Arg.Any<string>()).Returns(ct);
            service.Save(Arg.Any<IContentType>());

            var deliverable = new ContentTypeDeliverable(
                null,
                Substitute.For<TextWriter>(),
                service,
                null,
                null,
                null
            );

            await deliverable.Run("", new[] { "remove-property", "alias", "property-alias" });

            ct.Received(1).RemovePropertyType(Arg.Any<string>());
            service.Received(1).Save(Arg.Is(ct));
        }
    }
}