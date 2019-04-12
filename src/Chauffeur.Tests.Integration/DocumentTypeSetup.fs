module ``Document types``

open System.IO
open Xunit
open FsUnit.Xunit
open TestSamples
open System
open Chauffeur.TestingTools

type ``Importing document types``() =
    inherit UmbracoHostTestBase()
    let doctypeName = "blog-post"

    member private x.ImportDocType =
        async {
            let run = x.Host.RunWithArgs
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! _ = [| "install" |]
                            |> run
                            |> Async.AwaitTask
            return! [| "ct"; "import"; doctypeName |]
                    |> run
                    |> Async.AwaitTask
        }

    [<Fact>]
    member x.``Will log an error if you don't have the import file on disk``() =
        x.TextReader.AddCommand "Y"
        async {
            let! _ = x.ImportDocType
            let messages = x.TextWriter.Messages
            List.head messages |> should equal (sprintf "Unable to located the import script '%s'" doctypeName)
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Will import a document type successfully``() =
        x.TextReader.AddCommand "Y"
        let run = x.Host.RunWithArgs
        let chauffeurFolder = x.GetChauffeurFolder()
        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            sprintf "%s.xml" doctypeName |]
        File.WriteAllText(filePath, sampleDocType)
        async {
            let! _ = x.ImportDocType
            x.TextWriter.Flush()
            let! _ = [| "ct"; "get"; "BlogPost" |]
                     |> run
                     |> Async.AwaitTask
            let messages = x.TextWriter.Messages
            messages |> should haveLength 6
            let infoRow =
                messages
                |> List.rev
                |> List.skip 1
                |> List.head

            let parts = infoRow.Split([| '|' |], StringSplitOptions.RemoveEmptyEntries)
            parts.[0].Trim() |> should equal "1060"
            parts.[1].Trim() |> should equal "BlogPost"
            parts.[2].Trim() |> should equal "Blog Post"
            parts.[3].Trim() |> should equal "-1"
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Will import a document type successfully with updates``() =
        x.TextReader.AddCommand "Y"
        let chauffeurFolder = x.GetChauffeurFolder()
        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            sprintf "%s.xml" doctypeName |]
        File.WriteAllText(filePath, sampleDocType)

        let sampleDocTypeWithUpdates = "<?xml version=\"1.0\" encoding=\"utf-8\"?>
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
      <Name>Content is updated</Name>
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

        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            sprintf "%s-updated.xml" doctypeName |]
        File.WriteAllText(filePath, sampleDocTypeWithUpdates)
        async {
            let! _ = x.ImportDocType

            let run = x.Host.RunWithArgs
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! _ = [| "install" |]
                     |> run
                     |> Async.AwaitTask
            let! _=
                [| "ct"; "import"; sprintf "%s-updated" doctypeName |]
                |> run
                |> Async.AwaitTask
            x.TextWriter.Flush()
            let! _ = [| "ct"; "get"; "BlogPost" |]
                     |> run
                     |> Async.AwaitTask
            let messages = x.TextWriter.Messages
            messages |> should haveLength 6
            let infoRow =
                messages
                |> List.filter (fun m -> m.Contains("content"))
                |> List.head

            let parts = infoRow.Split([| '|' |], StringSplitOptions.RemoveEmptyEntries)
            parts.[0].Trim() |> should equal "50"
            parts.[1].Trim() |> should equal "Content is updated"
            parts.[2].Trim() |> should equal "content"
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Can remove a document type``() =
        x.TextReader.AddCommand "Y"
        let run = x.Host.RunWithArgs
        let chauffeurFolder = x.GetChauffeurFolder()
        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            sprintf "%s.xml" doctypeName |]
        File.WriteAllText(filePath, sampleDocType)

        async {
            let! _ = x.ImportDocType
            let! _ = [|"ct"; "remove"; "BlogPost" |]
                     |> run
                     |> Async.AwaitTask
            x.TextWriter.Flush()
            let! _ = [| "ct"; "get"; "BlogPost" |]
                     |> run
                     |> Async.AwaitTask
            x.TextWriter.Messages|> should haveLength 1
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Can remove a document type property``() =
        x.TextReader.AddCommand "Y"
        let run = x.Host.RunWithArgs
        let chauffeurFolder = x.GetChauffeurFolder()
        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            sprintf "%s.xml" doctypeName |]
        File.WriteAllText(filePath, sampleDocType)

        async {
            let! _ = x.ImportDocType
            let! _ = [|"ct"; "remove-property"; "BlogPost"; "introduction" |]
                      |> run
                      |> Async.AwaitTask
            x.TextWriter.Flush()
            let! _ = [| "ct"; "get"; "BlogPost" |]
                     |> run
                     |> Async.AwaitTask
            let messages = x.TextWriter.Messages
            messages |> should haveLength 5
        }
        |> Async.RunSynchronously