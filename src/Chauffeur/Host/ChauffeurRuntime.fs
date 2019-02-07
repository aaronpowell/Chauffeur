namespace Chauffeur.Host

open Umbraco.Core
open Umbraco.Core.Components
open Umbraco.Core.Composing
open Umbraco.Core.Runtime
open Umbraco.Web
open Chauffeur
open System.IO
open System.Reflection

type internal ChauffeurRuntime(reader : TextReader, writer : TextWriter) as self =
    inherit CoreRuntime()

    let mutable composition : Option<Composition> = None

    let getField name =
        self.GetType().BaseType.GetField(name, BindingFlags.Instance ||| BindingFlags.NonPublic)

    member private this.Hack register (factory : IFactory) composerTypes level =
        match level with
        | RuntimeLevel.BootFailed ->
            match composition with
            | Some c ->
                let ccb = new ComponentCollectionBuilder()
                ccb.RegisterWith(register)

                let composers = new Composers(c, composerTypes, this.ProfilingLogger)
                composers.Compose()

                //let _factory = getField "_factory"
                //_factory.SetValue(this, factory)
                //Current.Factory <- factory

                let _components = getField "_components"
                let cc = factory.GetInstance<ComponentCollection>()
                _components.SetValue(this, cc)
                cc.Initialize()
                factory
            | None -> factory
        | _ -> factory

    override __.Compose c = composition <- Some(c)

    override this.Boot(register) =
        register.Register(fun _ -> reader) |> ignore
        register.Register(fun _ -> writer) |> ignore
        register.Register<IHttpContextAccessor, NullHttpContextAccessor>() |> ignore
        register.Register(fun factory -> factory) |> ignore
        let factory = base.Boot(register)

        let runtimeState = factory.GetInstance<IRuntimeState>()
        let typeLoader = factory.GetInstance<TypeLoader>()
        this.Hack register factory (this.GetComposerTypes(typeLoader)) runtimeState.Level
