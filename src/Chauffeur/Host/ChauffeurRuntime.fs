namespace Chauffeur.Host

open Umbraco.Core.Runtime
open Umbraco.Web
open Chauffeur
open System.IO

type internal ChauffeurRuntime(reader : TextReader, writer : TextWriter) =
    inherit CoreRuntime()

    override __.Boot(c) =
        c.Register(fun _ -> reader) |> ignore
        c.Register(fun _ -> writer) |> ignore
        c.Register<IHttpContextAccessor, NullHttpContextAccessor>() |> ignore
        base.Boot(c)
