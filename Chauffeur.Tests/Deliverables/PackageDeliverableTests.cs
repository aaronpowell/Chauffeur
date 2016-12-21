using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Xml.Linq;
using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class PackageDeliverableTests
    {
        #region SampleDocumentTypesXml
        private const string multipleDocumentTypesXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<umbPackage>
  <DocumentTypes>
    <DocumentType>
      <Info>
        <Name>Demo</Name>
        <Alias>Demo</Alias>
        <Icon>.sprTreeFolder</Icon>
        <Thumbnail>folder.png</Thumbnail>
        <Description>
        </Description>
        <AllowAtRoot>False</AllowAtRoot>
        <AllowedTemplates />
        <DefaultTemplate>
        </DefaultTemplate>
      </Info>
      <Structure />
      <GenericProperties>
        <GenericProperty>
          <Name>Meta Data</Name>
          <Alias>metaData</Alias>
          <Type>Umbraco.Textbox</Type>
          <Definition>0cc0eba1-9960-42c9-bf9b-60e150b429ae</Definition>
          <Tab>
          </Tab>
          <Mandatory>False</Mandatory>
          <Validation>
          </Validation>
          <Description><![CDATA[]]></Description>
        </GenericProperty>
        <GenericProperty>
          <Name>Body Text</Name>
          <Alias>bodyText</Alias>
          <Type>Umbraco.TinyMCEv3</Type>
          <Definition>ca90c950-0aff-4e72-b976-a30b1ac57dad</Definition>
          <Tab>Content</Tab>
          <Mandatory>False</Mandatory>
          <Validation>
          </Validation>
          <Description><![CDATA[The body of a page]]></Description>
        </GenericProperty>
      </GenericProperties>
      <Tabs>
        <Tab>
          <Id>12</Id>
          <Caption>Content</Caption>
          <SortOrder>0</SortOrder>
        </Tab>
      </Tabs>
    </DocumentType>
    <DocumentType>
      <Info>
        <Name>Demo 2</Name>
        <Alias>Demo2</Alias>
        <Icon>.sprTreeFolder</Icon>
        <Thumbnail>folder.png</Thumbnail>
        <Description>
        </Description>
        <AllowAtRoot>False</AllowAtRoot>
        <AllowedTemplates />
        <DefaultTemplate>
        </DefaultTemplate>
      </Info>
      <Structure />
      <GenericProperties>
        <GenericProperty>
          <Name>Meta Data</Name>
          <Alias>metaData</Alias>
          <Type>ec15c1e5-9d90-422a-aa52-4f7622c63bea</Type>
          <Definition>0cc0eba1-9960-42c9-bf9b-60e150b429ae</Definition>
          <Tab>
          </Tab>
          <Mandatory>False</Mandatory>
          <Validation>
          </Validation>
          <Description><![CDATA[]]></Description>
        </GenericProperty>
        <GenericProperty>
          <Name>Body Text</Name>
          <Alias>bodyText</Alias>
          <Type>5e9b75ae-face-41c8-b47e-5f4b0fd82f83</Type>
          <Definition>ca90c950-0aff-4e72-b976-a30b1ac57dad</Definition>
          <Tab>Content</Tab>
          <Mandatory>False</Mandatory>
          <Validation>
          </Validation>
          <Description><![CDATA[The body of a page]]></Description>
        </GenericProperty>
      </GenericProperties>
      <Tabs>
        <Tab>
          <Id>14</Id>
          <Caption>Content</Caption>
          <SortOrder>0</SortOrder>
        </Tab>
      </Tabs>
    </DocumentType>
  </DocumentTypes>
</umbPackage>";

        private const string singleDocumentTypeXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentType>
  <Info>
    <Name>Content</Name>
    <Alias>Content</Alias>
    <Icon>icon-folders</Icon>
    <Thumbnail>folder.png</Thumbnail>
    <Description>Place holder for all Content related document types</Description>
    <AllowAtRoot>False</AllowAtRoot>
    <Master>RootDocType</Master>
    <AllowedTemplates />
    <DefaultTemplate></DefaultTemplate>
  </Info>
  <Structure />
  <GenericProperties>
    <GenericProperty>
      <Name>Foo name</Name>
      <Alias>fooName</Alias>
      <Type>Umbraco.Textbox</Type>
      <Definition>0cc0eba1-9960-42c9-bf9b-60e150b429ae</Definition>
      <Tab></Tab>
      <Mandatory>False</Mandatory>
      <Validation></Validation>
      <Description><![CDATA[]]></Description>
    </GenericProperty>
    <GenericProperty>
      <Name>Title</Name>
      <Alias>title</Alias>
      <Type>Umbraco.Textbox</Type>
      <Definition>0cc0eba1-9960-42c9-bf9b-60e150b429ae</Definition>
      <Tab>Content</Tab>
      <Mandatory>False</Mandatory>
      <Validation></Validation>
      <Description><![CDATA[Title of the page]]></Description>
    </GenericProperty>
    <GenericProperty>
      <Name>Content</Name>
      <Alias>bodyText</Alias>
      <Type>Umbraco.TinyMCEv3</Type>
      <Definition>ca90c950-0aff-4e72-b976-a30b1ac57dad</Definition>
      <Tab>Content</Tab>
      <Mandatory>False</Mandatory>
      <Validation></Validation>
      <Description><![CDATA[Content of the page]]></Description>
    </GenericProperty>
    <GenericProperty>
      <Name>Subject</Name>
      <Alias>metaSubject</Alias>
      <Type>Umbraco.Textbox</Type>
      <Definition>0cc0eba1-9960-42c9-bf9b-60e150b429ae</Definition>
      <Tab>Metadata</Tab>
      <Mandatory>False</Mandatory>
      <Validation></Validation>
      <Description><![CDATA[]]></Description>
    </GenericProperty>
    <GenericProperty>
      <Name>Robots</Name>
      <Alias>metaRobots</Alias>
      <Type>Umbraco.Textbox</Type>
      <Definition>0cc0eba1-9960-42c9-bf9b-60e150b429ae</Definition>
      <Tab>Metadata</Tab>
      <Mandatory>False</Mandatory>
      <Validation></Validation>
      <Description><![CDATA[]]></Description>
    </GenericProperty>
  </GenericProperties>
  <Tabs>
    <Tab>
      <Id>12</Id>
      <Caption>Content</Caption>
    </Tab>
    <Tab>
      <Id>13</Id>
      <Caption>Page Info</Caption>
    </Tab>
    <Tab>
      <Id>14</Id>
      <Caption>Metadata</Caption>
    </Tab>
  </Tabs>
</DocumentType>";
        #endregion

        #region Sample DataType
        private const string dataTypesXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<umbPackage>
  <DataTypes>
    <DataType Name='Date Picker with time' Id='Umbraco.DateTime' Definition='e4d66c0f-b935-4200-81f0-025f7256b89a' DatabaseType='Date'>
      <PreValues />
    </DataType>
    <DataType Name='Date Picker' Id='Umbraco.Date' Definition='5046194e-4237-453c-a547-15db3a07c4e1' DatabaseType='Date'>
      <PreValues />
    </DataType>
  </DataTypes>
</umbPackage>";
        #endregion

        #region Sample Templates XML
        private const string templatesXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<umbPackage>
  <Templates>
    <Template>
      <Name>AtoZPage</Name>
      <Alias>AtoZPage</Alias>
      <Master>SystemPages</Master>
      <Design><![CDATA[<!doctype html>
<html>
    <body>Template</body>
</html>]]></Design>
    </Template>
  </Templates>
</umbPackage>";
        #endregion

        #region Sample Macros XML
        private const string macrosXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<umbPackage>
  <Macros>
    <macro>
      <name>Render Venue Map</name>
      <alias>RenderVenueMap</alias>
      <scriptType>
      </scriptType>
      <scriptAssembly>
      </scriptAssembly>
      <xslt>
      </xslt>
      <useInEditor>False</useInEditor>
      <refreshRate>0</refreshRate>
      <scriptingFile>~/Views/MacroPartials/RenderVenueMap.cshtml</scriptingFile>
      <properties />
    </macro>
  </Macros>
</umbPackage>";
        #endregion

        [Fact]
        public async Task NoPackagesAbortsEarly()
        {
            var writer = Substitute.ForPartsOf<TextWriter>();
            var package = new PackageDeliverable(null, writer, null, null, null, null);

            await package.Run(null, new string[0]);

            writer.Received(1).WriteLineAsync(Arg.Any<string>()).IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task NotFoundPackageAbortsEarly()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x =>
            {
                x[0] = "";
                return true;
            });
            var package = new PackageDeliverable(null, writer, new MockFileSystem(), settings, null, null);

            await package.Run(null, new[] { "Test" });

            Assert.Equal(writer.Messages.Count(), 1);
        }

        [Fact]
        public async Task HavingMultipleDocumentTypesWillReadThemIn()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x =>
            {
                x[0] = "";
                return true;
            });
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "Text.xml", new MockFileData(multipleDocumentTypesXml) }
            });
            var packagingService = Substitute.For<IPackagingService>();
            packagingService.ImportContentTypes(Arg.Any<XElement>()).Returns(Enumerable.Empty<IContentType>());

            var package = new PackageDeliverable(null, writer, fs, settings, packagingService, Substitute.For<IContentTypeService>());

            await package.Run(null, new[] { "Text" });

            packagingService.Received(1).ImportContentTypes(Arg.Any<XElement>());
        }

        [Fact]
        public async Task HavingSingleDocumentTypeWillReadItIn()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x =>
            {
                x[0] = "";
                return true;
            });
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "Text.xml", new MockFileData(singleDocumentTypeXml) }
            });
            var packagingService = Substitute.For<IPackagingService>();
            packagingService.ImportContentTypes(Arg.Any<XElement>()).Returns(Enumerable.Empty<IContentType>());

            var package = new PackageDeliverable(null, writer, fs, settings, packagingService, Substitute.For<IContentTypeService>());

            await package.Run(null, new[] { "Text" });

            packagingService.Received(1).ImportContentTypes(Arg.Any<XElement>());
        }

        [Fact]
        public async Task HavingDataTypesWillReadThemIn()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x =>
            {
                x[0] = "";
                return true;
            });
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "Text.xml", new MockFileData(dataTypesXml) }
            });
            var packagingService = Substitute.For<IPackagingService>();
            packagingService.ImportDataTypeDefinitions(Arg.Any<XElement>()).Returns(Enumerable.Empty<IDataTypeDefinition>());

            var package = new PackageDeliverable(null, writer, fs, settings, packagingService, Substitute.For<IContentTypeService>());

            await package.Run(null, new[] { "Text" });

            packagingService.Received(2).ImportDataTypeDefinitions(Arg.Any<XElement>());
        }

        [Fact]
        public async Task HavingTemplatesWillReadThemIn()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x =>
            {
                x[0] = "";
                return true;
            });
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "Text.xml", new MockFileData(templatesXml) }
            });
            var packagingService = Substitute.For<IPackagingService>();
            packagingService.ImportTemplates(Arg.Any<XElement>()).Returns(Enumerable.Empty<ITemplate>());

            var package = new PackageDeliverable(null, writer, fs, settings, packagingService, Substitute.For<IContentTypeService>());

            await package.Run(null, new[] { "Text" });

            packagingService.Received(1).ImportTemplates(Arg.Any<XElement>());
        }

        [Fact]
        public async Task HavingMacrosWillReadThemIn()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x =>
            {
                x[0] = "";
                return true;
            });
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "Text.xml", new MockFileData(macrosXml) }
            });
            var packagingService = Substitute.For<IPackagingService>();
            packagingService.ImportMacros(Arg.Any<XElement>()).Returns(Enumerable.Empty<IMacro>());

            var package = new PackageDeliverable(null, writer, fs, settings, packagingService, Substitute.For<IContentTypeService>());

            await package.Run(null, new[] { "Text" });

            packagingService.Received(1).ImportMacros(Arg.Any<XElement>());
        }
    }
}
