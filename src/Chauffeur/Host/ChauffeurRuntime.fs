namespace Chauffeur.Host

open Umbraco.Core
open Umbraco.Core.Composing
open Umbraco.Core.Runtime
open Umbraco.Web
open Chauffeur
open System.IO
open System

type internal ChauffeurRuntime(reader : TextReader, writer : TextWriter) =
    inherit CoreRuntime()

    override __.Boot(register) =
        register.Register(reader)
        register.Register(writer)
        register.Register<IHttpContextAccessor, NullHttpContextAccessor>()
        register.Register<IFactory>(id)

        try
            base.Boot(register)
        with
            | :? InvalidOperationException -> Current.Factory