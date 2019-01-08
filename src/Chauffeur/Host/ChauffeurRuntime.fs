namespace Chauffeur.Host

open Umbraco.Core
open Umbraco.Core.Runtime
open Umbraco.Web
open Chauffeur
open System.IO
open LightInject

open System.Reflection
open System

module ChauffeurRuntime =
    let bootLoaderHack (c : ServiceContainer) componentTypes level =
        match level with
        | RuntimeLevel.BootFailed ->
            // time for reflection hacks!
            let bootLoaderType = level.GetType().Assembly.GetType("Umbraco.Core.Components.BootLoader")
            let bootLoader = Activator.CreateInstance(bootLoaderType, c)
            let method = bootLoaderType.GetMethod("Boot");
            try
                method.Invoke(bootLoader, [| componentTypes; RuntimeLevel.Install |]) |> ignore
            with _ ->
                ignore()
        | _ -> ignore()

type internal ChauffeurRuntime(reader : TextReader, writer : TextWriter) =
    inherit CoreRuntime()

    override this.Boot(c) =
        c.Register(fun _ -> reader) |> ignore
        c.Register(fun _ -> writer) |> ignore
        c.Register<IHttpContextAccessor, NullHttpContextAccessor>() |> ignore
        base.Boot(c)

        let runtimeState = c.GetInstance<IRuntimeState>()
        ChauffeurRuntime.bootLoaderHack c (this.GetComponentTypes()) runtimeState.Level
