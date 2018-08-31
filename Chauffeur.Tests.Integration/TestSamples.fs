module TestSamples

open System.Diagnostics.CodeAnalysis

[<SuppressMessage("SourceLength", "MaxLinesInValue")>]
let compositeDocTypeSample =
    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>
<umbPackage>
  <files />
  <info>
    <package>
      <name>InheritenceTest</name>
      <version>0.0.1</version>
      <iconUrl />
      <license url=\"http://opensource.org/licenses/MIT\">MIT License</license>
      <url>test.local</url>
      <requirements type=\"strict\">
        <major>7</major>
        <minor>5</minor>
        <patch>4</patch>
      </requirements>
    </package>
    <author>
      <name>Mark McDonald</name>
      <website>@MarkMcD27</website>
    </author>
    <readme><![CDATA[]]></readme>
  </info>
  <DocumentTypes>
    <DocumentType>
      <Info>
        <Name>Rich Text Page</Name>
        <Alias>richTextPage</Alias>
        <Icon>icon-document</Icon>
        <Thumbnail>folder.png</Thumbnail>
        <Description />
        <AllowAtRoot>False</AllowAtRoot>
        <IsListView>False</IsListView>
        <Compositions>
          <Composition>basePage</Composition>
        </Compositions>
        <AllowedTemplates>
          <Template>RichTextPage</Template>
        </AllowedTemplates>
        <DefaultTemplate>RichTextPage</DefaultTemplate>
      </Info>
      <Structure />
      <GenericProperties>
        <GenericProperty>
          <Name>Content</Name>
          <Alias>content</Alias>
          <Type>Umbraco.TinyMCEv3</Type>
          <Definition>ca90c950-0aff-4e72-b976-a30b1ac57dad</Definition>
          <Tab>Content</Tab>
          <SortOrder>1</SortOrder>
          <Mandatory>False</Mandatory>
        </GenericProperty>
      </GenericProperties>
      <Tabs>
        <Tab>
          <Id>17</Id>
          <Caption>Content</Caption>
          <SortOrder>0</SortOrder>
        </Tab>
      </Tabs>
    </DocumentType>
    <DocumentType>
      <Info>
        <Name>Base Page</Name>
        <Alias>basePage</Alias>
        <Icon>icon-document</Icon>
        <Thumbnail>folder.png</Thumbnail>
        <Description />
        <AllowAtRoot>False</AllowAtRoot>
        <IsListView>False</IsListView>
        <Compositions />
        <AllowedTemplates />
        <DefaultTemplate />
      </Info>
      <Structure />
      <GenericProperties>
        <GenericProperty>
          <Name>Title</Name>
          <Alias>title</Alias>
          <Type>Umbraco.Textbox</Type>
          <Definition>abee5b7d-42b4-4750-9a3d-c1af3173a853</Definition>
          <Tab>Content</Tab>
          <SortOrder>0</SortOrder>
          <Mandatory>False</Mandatory>
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
  </DocumentTypes>
  <Templates>
    <Template>
      <Name>Rich Text Page</Name>
      <Alias>RichTextPage</Alias>
      <Design><![CDATA[@inherits Umbraco.Web.Mvc.UmbracoViewPage<TemplatePaymentWebsite.Models.RichTextPageModel>
@{
	Layout = \"Shared/_Layout.cshtml\";
}

<div class=\"row\">
    <div  class=\"large-12 columns\">
        @Html.Raw(Model.Content)
    </div>
    </div>]]></Design>
    </Template>
  </Templates>
  <Stylesheets />
  <Macros />
  <DictionaryItems />
  <Languages />
  <DataTypes />
</umbPackage>"

let sampleDictionary = "<?xml version='1.0' encoding='UTF-8' standalone='no'?>
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
</umbPackage>"

let sampleDocType = "<?xml version=\"1.0\" encoding=\"utf-8\"?>
<DocumentType>
  <Info>
    <Name>Blog Post</Name>
    <Alias>BlogPost</Alias>
    <Icon>icon-edit</Icon>
    <Thumbnail>folder.png</Thumbnail>
    <Description></Description>
    <AllowAtRoot>False</AllowAtRoot>
    <IsListView>False</IsListView>
    <Compositions />
    <AllowedTemplates>
      <Template>BlogPost</Template>
    </AllowedTemplates>
    <DefaultTemplate>BlogPost</DefaultTemplate>
  </Info>
  <Structure />
  <GenericProperties>
    <GenericProperty>
      <Name>Content</Name>
      <Alias>content</Alias>
      <Type>Umbraco.Grid</Type>
      <Definition>a3785f08-73d5-406b-855f-8e52805c22e2</Definition>
      <Tab>Content</Tab>
      <SortOrder>0</SortOrder>
      <Mandatory>False</Mandatory>
      <Validation></Validation>
      <Description><![CDATA[]]></Description>
    </GenericProperty>
    <GenericProperty>
      <Name>Introduction</Name>
      <Alias>introduction</Alias>
      <Type>Umbraco.TextboxMultiple</Type>
      <Definition>c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3</Definition>
      <Tab>Introduction</Tab>
      <SortOrder>0</SortOrder>
      <Mandatory>False</Mandatory>
      <Validation></Validation>
      <Description><![CDATA[]]></Description>
    </GenericProperty>
  </GenericProperties>
  <Tabs>
    <Tab>
      <Id>12</Id>
      <Caption>Content</Caption>
      <SortOrder>2</SortOrder>
    </Tab>
    <Tab>
      <Id>13</Id>
      <Caption>Introduction</Caption>
      <SortOrder>1</SortOrder>
    </Tab>
  </Tabs>
</DocumentType>"

let preValuesPackage = ("<?xml version='1.0' encoding='UTF-8' standalone='no'?>
<umbPackage>
  <files />
  <info>
  </info>
  <DocumentTypes />
  <Templates />
  <Stylesheets />
  <Macros />
  <DictionaryItems />
  <Languages />
  <DataTypes>
    <DataType Name='Articulate Cropper' Id='Umbraco.ImageCropper' Definition='c8f535ee-27b8-4d16-940d-d6c523851bb1' DatabaseType='Ntext'>
      <PreValues>
        <PreValue Id='65' Value='[{&quot;alias&quot;:&quot;blogPost&quot;,&quot;width&quot;:200,&quot;height&quot;:200},{&quot;alias&quot;:&quot;thumbnail&quot;,&quot;width&quot;:50,&quot;height&quot;:50},{&quot;alias&quot;:&quot;square&quot;,&quot;width&quot;:480,&quot;height&quot;:480},{&quot;alias&quot;:&quot;wide&quot;,&quot;width&quot;:1024,&quot;height&quot;:512}]' Alias='crops' SortOrder='0' />
      </PreValues>
    </DataType>
  </DataTypes>
</umbPackage>", "<?xml version='1.0' encoding='UTF-8' standalone='no'?>
<umbPackage>
  <files />
  <info>
  </info>
  <DocumentTypes />
  <Templates />
  <Stylesheets />
  <Macros />
  <DictionaryItems />
  <Languages />
  <DataTypes>
    <DataType Name='Articulate Cropper' Id='Umbraco.ImageCropper' Definition='c8f535ee-27b8-4d16-940d-d6c523851bb1' DatabaseType='Ntext'>
      <PreValues>
        <PreValue Id='71' Value='[&#xD;&#xA;  {&#xD;&#xA;    &quot;alias&quot;: &quot;blogPost&quot;,&#xD;&#xA;    &quot;width&quot;: 200,&#xD;&#xA;    &quot;height&quot;: 200&#xD;&#xA;  },&#xD;&#xA;  {&#xD;&#xA;    &quot;alias&quot;: &quot;thumbnail&quot;,&#xD;&#xA;    &quot;width&quot;: 50,&#xD;&#xA;    &quot;height&quot;: 50&#xD;&#xA;  },&#xD;&#xA;  {&#xD;&#xA;    &quot;width&quot;: 400,&#xD;&#xA;    &quot;height&quot;: 200,&#xD;&#xA;    &quot;alias&quot;: &quot;relatedPost&quot;&#xD;&#xA;  },&#xD;&#xA;  {&#xD;&#xA;    &quot;alias&quot;: &quot;postHeader&quot;,&#xD;&#xA;    &quot;width&quot;: 1600,&#xD;&#xA;    &quot;height&quot;: 400&#xD;&#xA;  }&#xD;&#xA;]' Alias='crops' SortOrder='0' />
      </PreValues>
    </DataType>
  </DataTypes>
</umbPackage>")

let filesPackage = sprintf "<?xml version='1.0' encoding='UTF-8' standalone='no'?>
<umbPackage>
  <files>
    <file>
      <guid>454ccf67-8ffe-493c-b6b0-ab1c5f8554d0_foo.css</guid>
      <orgPath>%scss</orgPath>
      <orgName>foo.css</orgName>
    </file>
  </files>
  <info>
  </info>
  <DocumentTypes />
  <Templates />
  <Stylesheets />
  <Macros />
  <DictionaryItems />
  <Languages />
  <DataTypes />
</umbPackage>"