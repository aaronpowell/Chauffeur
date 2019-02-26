namespace Chauffeur.Components

open System
open System.Reflection
open Chauffeur
open Umbraco.Core.Composing

type Registration =
     { Type : Type
       KnownAs : seq<string> }

module internal ChauffeurDeliverableComponent =
    let deliveryType = typeof<Deliverable>

    let registerDeliverable (register : IRegister) (t : Type) =
        let name = t.GetCustomAttribute<DeliverableNameAttribute>().Name
        register.Register(t, Lifetime.Transient)
        register.Register((fun factory -> (factory.GetInstance t) :?> Deliverable), Lifetime.Transient)

        let aliases = t.GetCustomAttributes<DeliverableAliasAttribute>()
                      |> Seq.map (fun attr -> attr.Alias)
                      |> Seq.toArray

        { Type = t
          KnownAs = Array.append [| name |] aliases }

type DeliverableResolver(factory : IFactory, registrations) =
    let finder registrations key =
        registrations |> Array.tryFind (fun r -> let x = r.KnownAs |> Seq.tryFind (fun a -> a = key)
                                                 match x with
                                                 | Some _ -> true
                                                 | None _ -> false)
    member __.Resolve key =
        let registration = finder registrations key
        match registration with
        | Some r -> Some(factory.GetInstance r.Type :?> Deliverable)
        | None -> None

open ChauffeurDeliverableComponent

[<RuntimeLevelAttribute(MinLevel = Umbraco.Core.RuntimeLevel.BootFailed)>]
type ChauffeurDeliverableComponent() =
    interface IComposer with
        member __.Compose(composition) =
            let typeLoader = composition.TypeLoader

            let registrations = typeLoader.GetTypes<Deliverable>()
                                |> Seq.map (registerDeliverable composition)
                                |> Seq.toArray

            composition.Register(fun factory -> DeliverableResolver(factory, registrations))
