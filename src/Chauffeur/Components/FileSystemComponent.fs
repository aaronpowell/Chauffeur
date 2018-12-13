namespace Chauffeur.Component

open Umbraco.Core.Components
open System.IO.Abstractions

type FileSystemComponent() =
    inherit UmbracoComponentBase()

    override __.Compose(c) =
        c.Container.Register<IFileSystem, FileSystem>() |> ignore