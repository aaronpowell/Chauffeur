using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class ScaffoldDeliverableTests
    {
        [Fact]
        public async Task ScaffoldWillSetupDeliveryFile()
        {
            var reader = Substitute.For<TextReader>();
            reader.ReadLineAsync().Returns(Task.FromResult(""));

            var writer = new MockTextWriter();
            var fileSystem = new MockFileSystem();
            var settings = Substitute.For<IChauffeurSettings>();
            settings.TryGetChauffeurDirectory(out string s).Returns((x) =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var deliverable = new ScaffoldDeliverable(reader, writer, settings, fileSystem);

            await deliverable.Run("", new string[0]);

            Assert.Single(fileSystem.AllFiles);
            Assert.Contains("001-Setup.delivery", fileSystem.AllFiles.First());
        }

        [Fact]
        public async Task DefaultDeliveryFillIncludeInstallAndChangePassword()
        {
            var reader = Substitute.For<TextReader>();
            reader.ReadLineAsync().Returns(Task.FromResult(""));

            var writer = new MockTextWriter();
            var fileSystem = new MockFileSystem();
            var settings = Substitute.For<IChauffeurSettings>();
            settings.TryGetChauffeurDirectory(out string s).Returns((x) =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var deliverable = new ScaffoldDeliverable(reader, writer, settings, fileSystem);

            await deliverable.Run("", new string[0]);

            var file = fileSystem.GetFile(@"c:\foo\001-Setup.delivery");
            var content = Encoding.Default.GetString(file.Contents).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal("install y", content[0]);
            Assert.Equal("user change-password admin $adminpwd$", content[1]);
        }
    }
}
