using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Chauffeur.Tests.Deliverables
{
    [TestFixture]
    class InstallDeliverableTests
    {
        [Test]
        public async Task MissingConnectionString_WillWarnAndExit()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            var deliverable = new InstallDeliverable(null, writer, null, settings, null, null);

            await deliverable.Run(null, null);

            Assert.That(writer.Messages.Count(), Is.EqualTo(1));
        }
    }
}
