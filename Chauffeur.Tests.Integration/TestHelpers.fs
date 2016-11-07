module TestHelpers

open System
open System.IO
open System.Reflection
open Chauffeur.Tests.Integration
open Chauffeur.Host

let private cwd = FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName
let private dbFolder = "databases"

let private setDataDirectory() =
    let now = DateTimeOffset.Now
    let ticks = now.Ticks.ToString()

    let folderForRun = Path.Combine [|cwd; dbFolder; ticks|]

    Directory.CreateDirectory folderForRun |> ignore

    AppDomain.CurrentDomain.SetData("DataDirectory", folderForRun)

    folderForRun

let knownTables =
    ["cmsContent";
    "cmsContentType";
    "cmsContentType2ContentType";
    "cmsContentTypeAllowedContentType";
    "cmsContentVersion";
    "cmsContentXml";
    "cmsDataType";
    "cmsDataTypePreValues";
    "cmsDictionary";
    "cmsDocument";
    "cmsDocumentType";
    "cmsLanguageText";
    "cmsMacro";
    "cmsMacroProperty";
    "cmsMember";
    "cmsMember2MemberGroup";
    "cmsMemberType";
    "cmsPreviewXml";
    "cmsPropertyData";
    "cmsPropertyType";
    "cmsPropertyTypeGroup";
    "cmsTagRelationship";
    "cmsTags";
    "cmsTask";
    "cmsTaskType";
    "cmsTemplate";
    "umbracoAccess";
    "umbracoAccessRule";
    "umbracoCacheInstruction";
    "umbracoDeployChecksum";
    "umbracoDeployDependency";
    "umbracoDomains";
    "umbracoExternalLogin";
    "umbracoLanguage";
    "umbracoLog";
    "umbracoMigration";
    "umbracoNode";
    "umbracoRedirectUrl";
    "umbracoRelation";
    "umbracoRelationType";
    "umbracoServer";
    "umbracoUser";
    "umbracoUser2app";
    "umbracoUser2NodeNotify";
    "umbracoUser2NodePermission";
    "umbracoUserType"]

[<AbstractClass>]
type UmbracoHostTestBase() = 
    let dbFolder = setDataDirectory()

    let writer = new MockTextWriter()
    let reader = new MockTextReader()
    let host = new UmbracoHost(reader, writer)

    member x.DatabaseLocation = dbFolder
    member x.Host = host
    member x.TextReader = reader
    member x.TextWriter = writer

    interface IDisposable with
        member x.Dispose() =
            writer.Dispose()
            reader.Dispose()
            host.Dispose()
