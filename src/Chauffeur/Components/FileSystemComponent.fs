namespace Chauffeur.Components

open Umbraco.Core.Components
open Umbraco.Core.Composing
open System.IO.Abstractions

[<RuntimeLevelAttribute(MinLevel = Umbraco.Core.RuntimeLevel.BootFailed)>]
type FileSystemComponent() =
    interface IComposer with
        member __.Compose(register) =
            register.Register<IFileSystem, FileSystem>()