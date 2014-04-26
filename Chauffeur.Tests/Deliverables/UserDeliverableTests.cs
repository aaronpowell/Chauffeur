using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Chauffeur.Deliverables;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Core.Security;

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
        public async Task NonNumericalUserIdWillCauseAnError()
        {
            var writer = new MockTextWriter();
            var deliverable = new UserDeliverable(null, writer, null);

            await deliverable.Run("user", new[] { "change-password", "a", "a", "a" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task UserIdNotMatchingAnyWillCauseAnError()
        {
            var writer = new MockTextWriter();
            var provider = Substitute.For<UmbracoMembershipProviderBase>();
            provider.GetUser(Arg.Any<int>(), false).Returns((MembershipUser)null);

            var deliverable = new UserDeliverable(null, writer, provider);

            await deliverable.Run("user", new[] { "change-password", "0", "a", "a" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
            provider.Received(1).GetUser(Arg.Any<int>(), false);
        }

        [Test]
        public async Task ValidUserWillHaveTheirPasswordUpdated()
        {
            var writer = new MockTextWriter();
            var provider = Substitute.For<UmbracoMembershipProviderBase>();
            var user = Substitute.For<MembershipUser>();
            user.ChangePassword("a", "ab").Returns(true);
            provider.GetUser(Arg.Any<int>(), false).Returns(user);

            var deliverable = new UserDeliverable(null, writer, provider);

            await deliverable.Run("user", new[] { "change-password", "0", "a", "ab" });

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
            user.Received(1).ChangePassword("a", "ab");
        }
    }
}
