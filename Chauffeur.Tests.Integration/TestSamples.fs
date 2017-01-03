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