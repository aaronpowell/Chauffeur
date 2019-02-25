namespace Chauffeur.Components

open Umbraco.Core.Composing
open System.IO.Abstractions
open CompositionExtensions

[<RuntimeLevelAttribute(MinLevel = Umbraco.Core.RuntimeLevel.BootFailed)>]
type FileSystemComponent() =
    interface IComposer with
        member __.Compose(register) =
            register.RegisterAs<IFileSystem, FileSystem>()