module TestHelpers
open System
open System.IO
open System.Reflection

let cwd = FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName
let dbFolder = "databases"

let setDataDirectory() =
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