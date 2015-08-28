using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Chauffeur.Deliverables;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Chauffeur.Tests.Deliverables
{
    [TestFixture]
    class UserDeliverableTests
    {
        [Test]
        public async Task NoArgumentsWillWriteOutMessage()
        {
            var writer = new MockTextWriter();
            var deliverable = new UserDeliverable(null, writer, null);

            await deliverable.Run("user", new string[0]);

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task ChangePasswordWillFailIfExpectedArgumentsAreNotProvided()
        {
            var writer = new MockTextWriter();
            var deliverable = new UserDeliverable(null, writer, null);

            await deliverable.Run("user", new[] { "change-password" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task UserIdNotMatchingAnyWillCauseAnError()
        {
            var writer = new MockTextWriter();
            var userService = Substitute.For<IUserService>();
            userService.GetByUsername(Arg.Any<string>()).Returns((IUser)null);

            var deliverable = new UserDeliverable(null, writer, userService);

            await deliverable.Run("user", new[] { "change-password", "0", "a" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
            userService.Received(1).GetByUsername(Arg.Any<string>());
        }

        [Test]
        public async Task ValidUserWillHaveTheirPasswordUpdated()
        {
            var writer = new MockTextWriter();
            var userService = Substitute.For<IUserService>();
            var user = Substitute.For<IUser>();
            userService.GetByUsername(Arg.Any<string>()).Returns(user);

            var deliverable = new UserDeliverable(null, writer, userService);

            await deliverable.Run("user", new[] { "change-password", "0", "ab" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
            userService.Received(1).SavePassword(user, "ab");
        }
    }
}
