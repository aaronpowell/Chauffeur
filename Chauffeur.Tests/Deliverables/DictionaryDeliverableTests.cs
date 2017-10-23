using Chauffeur.Deliverables;
using Chauffeur.Host;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core.Services;
using Xunit;

namespace Chauffeur.Tests.Deliverables
{
    public class DictionaryDeliverableTests
    {
        #region SampleDictionary
        private const string packageWithLangsXml = @"<?xml version='1.0' encoding='UTF-8' standalone='no'?>
<umbPackage>
  <files />
  <info>
    <package>
      <name>DictionaryTest</name>
      <version>1</version>
      <iconUrl />
      <license url='None'>None</license>
      <url>local</url>
      <requirements type='strict'>
        <major>7</major>
        <minor>5</minor>
        <patch>4</patch>
      </requirements>
    </package>
    <author>
      <name>Mark M</name>
      <website>@MarkMcD27</website>
    </author>
    <readme><![CDATA[]]></readme>
  </info>
  <DocumentTypes />
  <Templates />
  <Stylesheets />
  <Macros />
  <DictionaryItems>
    <DictionaryItem Key='TestTwo'>
      <DictionaryItem Key='TestTwo.Child2'>
        <Value LanguageId='1' LanguageCultureAlias='en-US'><![CDATA[Test two Child 2 US]]></Value>
        <Value LanguageId='2' LanguageCultureAlias='en-GB'><![CDATA[Test two Child 2 UK]]></Value>
      </DictionaryItem>
      <DictionaryItem Key='TestTwo.Child1'>
        <Value LanguageId='1' LanguageCultureAlias='en-US'><![CDATA[Test two Child 1 US]]></Value>
        <Value LanguageId='2' LanguageCultureAlias='en-GB'><![CDATA[Test two Child 1 UK]]></Value>
      </DictionaryItem>
    </DictionaryItem>
    <DictionaryItem Key='TestOne'>
      <Value LanguageId='1' LanguageCultureAlias='en-US'><![CDATA[Test One US]]></Value>
      <Value LanguageId='2' LanguageCultureAlias='en-GB'><![CDATA[Test One UK]]></Value>
    </DictionaryItem>
  </DictionaryItems>
  <Languages>
    <Language Id='1' CultureAlias='en-US' FriendlyName='en-US' />
    <Language Id='2' CultureAlias='en-GB' FriendlyName='English (United Kingdom)' />
  </Languages>
  <DataTypes />
</umbPackage>";
        #endregion

        #region SampleDictionaryNoLanguages
        private static readonly string packageNoLangXml = @"<?xml version='1.0' encoding='UTF-8' standalone='no'?>
<umbPackage>
  <files />
  <info>
    <package>
      <name>DictionaryTest</name>
      <version>1</version>
      <iconUrl />
      <license url='None'>None</license>
      <url>local</url>
      <requirements type='strict'>
        <major>7</major>
        <minor>5</minor>
        <patch>4</patch>
      </requirements>
    </package>
    <author>
      <name>Mark M</name>
      <website>@MarkMcD27</website>
    </author>
    <readme><![CDATA[]]></readme>
  </info>
  <DocumentTypes />
  <Templates />
  <Stylesheets />
  <Macros />
  <DictionaryItems>
    <DictionaryItem Key='TestTwo'>
      <DictionaryItem Key='TestTwo.Child2'>
        <Value LanguageId='1' LanguageCultureAlias='en-US'><![CDATA[Test two Child 2 US]]></Value>
        <Value LanguageId='2' LanguageCultureAlias='en-GB'><![CDATA[Test two Child 2 UK]]></Value>
      </DictionaryItem>
      <DictionaryItem Key='TestTwo.Child1'>
        <Value LanguageId='1' LanguageCultureAlias='en-US'><![CDATA[Test two Child 1 US]]></Value>
        <Value LanguageId='2' LanguageCultureAlias='en-GB'><![CDATA[Test two Child 1 UK]]></Value>
      </DictionaryItem>
    </DictionaryItem>
    <DictionaryItem Key='TestOne'>
      <Value LanguageId='1' LanguageCultureAlias='en-US'><![CDATA[Test One US]]></Value>
      <Value LanguageId='2' LanguageCultureAlias='en-GB'><![CDATA[Test One UK]]></Value>
    </DictionaryItem>
  </DictionaryItems>
  <DataTypes />
</umbPackage>";
        #endregion

        [Fact]
        public async Task DeliverableExitsIfPackageNotFound()
        {
            var writer = new MockTextWriter();
            var settings = Substitute.For<IChauffeurSettings>();
            string dir;
            settings.TryGetChauffeurDirectory(out dir).Returns(x =>
            {
                x[0] = "";
                return true;
            });
            var package = new DictionaryDeliverable(null, writer, new MockFileSystem(), settings, null);

            await package.Run(null, new[] { "Test" });

            Assert.Single(writer.Messages);
        }

        [Fact]
        public async Task NoLanguageArgSkipsLanguageCall()
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
                { "Text.xml", new MockFileData(packageWithLangsXml) }
            });

            var packagingService = Substitute.For<IPackagingService>();
            packagingService.ImportLanguages(Arg.Any<XElement>()).Returns(Enumerable.Empty<Umbraco.Core.Models.ILanguage>());
            packagingService.ImportDictionaryItems(Arg.Any<XElement>()).Returns(Enumerable.Empty<Umbraco.Core.Models.IDictionaryItem>());

            var package = new DictionaryDeliverable(null, writer, fs, settings, packagingService);

            await package.Run(null, new[] { "Text" });

            packagingService.Received(0).ImportLanguages(Arg.Any<XElement>());
            packagingService.Received(1).ImportDictionaryItems(Arg.Any<XElement>());

        }

        [Fact]
        public async Task LanguageArgLoadsLanguages()
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
                { "Text.xml", new MockFileData(packageWithLangsXml) }
            });

            var packagingService = Substitute.For<IPackagingService>();
            packagingService.ImportLanguages(Arg.Any<XElement>()).Returns(Enumerable.Empty<Umbraco.Core.Models.ILanguage>());
            packagingService.ImportDictionaryItems(Arg.Any<XElement>()).Returns(Enumerable.Empty<Umbraco.Core.Models.IDictionaryItem>());

            var package = new DictionaryDeliverable(null, writer, fs, settings, packagingService);

            await package.Run(null, new[] { "Text", "y" });

            packagingService.Received(1).ImportLanguages(Arg.Any<XElement>());
            packagingService.Received(1).ImportDictionaryItems(Arg.Any<XElement>());
        }

        [Fact]
        public async Task DictionaryItemsLoadedIfNoLanguagesAreFound()
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
                { "Text.xml", new MockFileData(packageNoLangXml) }
            });

            var packagingService = Substitute.For<IPackagingService>();
            packagingService.ImportLanguages(Arg.Any<XElement>()).Returns(Enumerable.Empty<Umbraco.Core.Models.ILanguage>());
            packagingService.ImportDictionaryItems(Arg.Any<XElement>()).Returns(Enumerable.Empty<Umbraco.Core.Models.IDictionaryItem>());

            var package = new DictionaryDeliverable(null, writer, fs, settings, packagingService);

            await package.Run(null, new[] { "Text", "y" });

            packagingService.Received(0).ImportLanguages(Arg.Any<XElement>());
            packagingService.Received(1).ImportDictionaryItems(Arg.Any<XElement>());
        }
    }
}
