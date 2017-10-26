using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Xunit;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Chauffeur.Tests.Deliverables
{
    public class ScaffoldDeliverableTests
    {
        [Fact]
        public async Task ScaffoldWillSetupDeliveryFile()
        {
            var reader = Substitute.For<TextReader>();
            reader.ReadLineAsync().Returns(Task.FromResult(""), Task.FromResult(""), Task.FromResult("n"));

            var writer = new MockTextWriter();
            var fileSystem = new MockFileSystem();
            var settings = Substitute.For<IChauffeurSettings>();
            settings.TryGetChauffeurDirectory(out string s).Returns((x) =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var deliverable = new ScaffoldDeliverable(
                reader,
                writer,
                settings,
                fileSystem,
                null,
                null,
                null,
                null,
                null);

            await deliverable.Run("", new string[0]);

            Assert.Single(fileSystem.AllFiles);
            Assert.Contains("001-Setup.delivery", fileSystem.AllFiles.First());
        }

        [Fact]
        public async Task DefaultDeliveryFillIncludeInstallAndChangePassword()
        {
            var reader = Substitute.For<TextReader>();
            reader.ReadLineAsync().Returns(Task.FromResult(""), Task.FromResult(""), Task.FromResult("n"));

            var writer = new MockTextWriter();
            var fileSystem = new MockFileSystem();
            var settings = Substitute.For<IChauffeurSettings>();
            settings.TryGetChauffeurDirectory(out string s).Returns((x) =>
            {
                x[0] = @"c:\foo";
                return true;
            });

            var deliverable = new ScaffoldDeliverable(
                reader,
                writer,
                settings,
                fileSystem,
                null,
                null,
                null,
                null,
                null);

            await deliverable.Run("", new string[0]);

            var file = fileSystem.GetFile(@"c:\foo\001-Setup.delivery");
            var content = file.TextContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal("install y", content[0]);
            Assert.Equal("user change-password admin $adminpwd$", content[1]);
        }

        [Fact]
        public async Task WhenExportingPackageItIsAddedToTheDelivery()
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

            var contentTypeService = Substitute.For<IContentTypeService>();
            contentTypeService.GetAllContentTypes().ReturnsForAnyArgs(Enumerable.Empty<IContentType>());
            var dataTypeService = Substitute.For<IDataTypeService>();
            dataTypeService.GetAllDataTypeDefinitions().ReturnsForAnyArgs(Enumerable.Empty<IDataTypeDefinition>());
            var fileService = Substitute.For<IFileService>();
            fileService.GetTemplates().ReturnsForAnyArgs(Enumerable.Empty<ITemplate>());
            fileService.GetStylesheets().ReturnsForAnyArgs(Enumerable.Empty<Stylesheet>());
            var macroService = Substitute.For<IMacroService>();
            macroService.GetAll().ReturnsForAnyArgs(Enumerable.Empty<IMacro>());
            var packagingService = Substitute.For<IPackagingService>();

            var deliverable = new ScaffoldDeliverable(
                reader,
                writer,
                settings,
                fileSystem,
                contentTypeService,
                dataTypeService,
                fileService,
                macroService,
                packagingService);

            await deliverable.Run("", new string[0]);

            var file = fileSystem.GetFile(@"c:\foo\001-Setup.delivery");
            var content = file.TextContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal("package 001-Setup", content[2]);
        }

        [Fact]
        public async Task WhenExportingPackageItCreatesAnXmlFile()
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

            var contentTypeService = Substitute.For<IContentTypeService>();
            contentTypeService.GetAllContentTypes().ReturnsForAnyArgs(Enumerable.Empty<IContentType>());
            var dataTypeService = Substitute.For<IDataTypeService>();
            dataTypeService.GetAllDataTypeDefinitions().ReturnsForAnyArgs(Enumerable.Empty<IDataTypeDefinition>());
            var fileService = Substitute.For<IFileService>();
            fileService.GetTemplates().ReturnsForAnyArgs(Enumerable.Empty<ITemplate>());
            fileService.GetStylesheets().ReturnsForAnyArgs(Enumerable.Empty<Stylesheet>());
            var macroService = Substitute.For<IMacroService>();
            macroService.GetAll().ReturnsForAnyArgs(Enumerable.Empty<IMacro>());
            var packagingService = Substitute.For<IPackagingService>();

            var deliverable = new ScaffoldDeliverable(
                reader,
                writer,
                settings,
                fileSystem,
                contentTypeService,
                dataTypeService,
                fileService,
                macroService,
                packagingService);

            await deliverable.Run("", new string[0]);

            var file = fileSystem.GetFile(@"c:\foo\001-Setup.xml");
            Assert.NotNull(file);
        }
    }
}
