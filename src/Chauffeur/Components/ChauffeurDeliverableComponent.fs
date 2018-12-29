namespace Chauffeur.Components

open System
open System.Reflection
open Chauffeur
open LightInject
open Umbraco.Core.Components
open Umbraco.Core.Composing

module internal ChauffeurDeliverableComponent =
    let nameBuilder name = sprintf "chauffeur:%s" name

    let deliveryType = typeof<Deliverable>

    let resolver deliverableName (factory : IServiceFactory) =
        factory.GetInstance<Deliverable>(nameBuilder deliverableName)

    let registerDeliverable (container : IServiceContainer) (t : Type) =
        let name = t.GetCustomAttribute<DeliverableNameAttribute>().Name
        container.Register(deliveryType, t, nameBuilder name) |> ignore

        t.GetCustomAttributes<DeliverableAliasAttribute>()
        |> Seq.iter (fun attr ->
                container
                    .Register<Deliverable>(
                        resolver name,
                        nameBuilder attr.Alias
                    )
                |> ignore
            )

open ChauffeurDeliverableComponent

type ChauffeurDeliverableComponent() =
    inherit UmbracoComponentBase()

    override __.Compose(composition) =
        let typeLoader = composition.Container.GetInstance<TypeLoader>()

        typeLoader.GetTypes<Deliverable>()
        |> Seq.iter (registerDeliverable composition.Container)
