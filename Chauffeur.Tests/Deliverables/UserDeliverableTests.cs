using System.Linq;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using NSubstitute;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class UserDeliverableTests
    {
        [Fact]
        public async Task NoArgumentsWillWriteOutMessage()
        {
            var writer = new MockTextWriter();
            var deliverable = new UserDeliverable(null, writer, null);

            await deliverable.Run("user", new string[0]);

            Assert.Single(writer.Messages);
        }

        [Fact]
        public async Task ChangePasswordWillFailIfExpectedArgumentsAreNotProvided()
        {
            var writer = new MockTextWriter();
            var deliverable = new UserDeliverable(null, writer, null);

            await deliverable.Run("user", new[] { "change-password" });

            Assert.Equal(2, writer.Messages.Count());
        }

        [Fact]
        public async Task ChangeUserWillFailIfExpectedArgumentsAreNotProvided()
        {
            var writer = new MockTextWriter();
            var deliverable = new UserDeliverable(null, writer, null);

            await deliverable.Run("user", new[] { "change-name" });

            Assert.Equal(2, writer.Messages.Count());
        }

        [Fact]
        public async Task ChangeUserNameWillFailIfExpectedArgumentsAreNotProvided()
        {
            var writer = new MockTextWriter();
            var deliverable = new UserDeliverable(null, writer, null);

            await deliverable.Run("user", new[] { "change-loginname" });

            Assert.Equal(2, writer.Messages.Count());
        }

        [Fact]
        public async Task UserIdNotMatchingAnyWillCauseAnError()
        {
            var writer = new MockTextWriter();
            var userService = Substitute.For<IUserService>();
            userService.GetByUsername(Arg.Any<string>()).Returns((IUser)null);

            var deliverable = new UserDeliverable(null, writer, userService);

            await deliverable.Run("user", new[] { "change-password", "0", "a" });

            Assert.Single(writer.Messages);
            userService.Received(1).GetByUsername(Arg.Any<string>());
        }

        [Fact]
        public async Task UserIdForChangeNameNotMatchingAnyWillCauseAnError()
        {
            var writer = new MockTextWriter();
            var userService = Substitute.For<IUserService>();
            userService.GetByUsername(Arg.Any<string>()).Returns((IUser)null);

            var deliverable = new UserDeliverable(null, writer, userService);

            await deliverable.Run("user", new[] { "change-name", "0", "b" });

            Assert.Single(writer.Messages);
            userService.Received(1).GetByUsername(Arg.Any<string>());
        }
        [Fact]
        public async Task UserIdForChangeUserNameNotMatchingAnyWillCauseAnError()
        {
            var writer = new MockTextWriter();
            var userService = Substitute.For<IUserService>();
            userService.GetByUsername(Arg.Any<string>()).Returns((IUser)null);

            var deliverable = new UserDeliverable(null, writer, userService);

            await deliverable.Run("user", new[] { "change-loginname", "0", "c" });

            Assert.Single(writer.Messages);
            userService.Received(1).GetByUsername(Arg.Any<string>());
        }
        [Fact]
        public async Task ValidUserWillHaveTheirPasswordUpdated()
        {
            var writer = new MockTextWriter();
            var userService = Substitute.For<IUserService>();
            var user = Substitute.For<IUser>();
            userService.GetByUsername(Arg.Any<string>()).Returns(user);

            var deliverable = new UserDeliverable(null, writer, userService);

            await deliverable.Run("user", new[] { "change-password", "0", "ab" });

            Assert.Single(writer.Messages);
            userService.Received(1).SavePassword(user, "ab");
        }

        [Fact]
        public async Task ValidUserWillHaveTheirNameUpdated()
        {
            var writer = new MockTextWriter();
            var userService = Substitute.For<IUserService>();
            var user = Substitute.For<IUser>();
            user.Name = "a";
            userService.GetByUsername("a").Returns(user);

            var deliverable = new UserDeliverable(null, writer, userService);

            await deliverable.Run("user", new[] { "change-name", "a", "ab" });

            Assert.Single(writer.Messages);
            Assert.Equal("ab", user.Name);
            userService.Received(1).Save(user);
        }

        [Fact]
        public async Task ValidUserWillHaveTheirUserNameUpdated()
        {
            var writer = new MockTextWriter();
            var userService = Substitute.For<IUserService>();
            var user = Substitute.For<IUser>();
            user.Name = "a";
            user.Username = "a";
            userService.GetByUsername("a").Returns(user);

            var deliverable = new UserDeliverable(null, writer, userService);

            await deliverable.Run("user", new[] { "change-loginname", "a", "ab" });

            Assert.Single(writer.Messages);
            Assert.Equal("a", user.Name);
            Assert.Equal("ab", user.Username);
            userService.Received(1).Save(user);
        }
    }
}
