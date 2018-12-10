namespace Chauffeur

open LightInject
open System.Reflection
open Umbraco.Core.Components
open Umbraco.Core.Composing

type ChauffeurDeliverableComponent() =
    inherit UmbracoComponentBase()

    let deliveryType = typeof<Deliverable>

    let nameBuilder name = sprintf "chauffeur:%s" name

    let register (container : IServiceContainer) ``type`` name =
        container.Register(deliveryType, ``type``, nameBuilder name)

    override __.Compose(composition) =
        let typeLoader = composition.Container.GetInstance<TypeLoader>()

        let register' = register composition.Container

        typeLoader.GetTypes<Deliverable>()
        |> Seq.iter (fun t ->
                let nameAttr = t.GetCustomAttribute<DeliverableNameAttribute>()
                register' t nameAttr.Name
                |> ignore

                t.GetCustomAttributes<DeliverableAliasAttribute>()
                |> Seq.iter (fun attr ->
                        let resolver = fun (factory : IServiceFactory) ->
                            factory.GetInstance<Deliverable>(nameBuilder nameAttr.Name)
                        composition.Container
                            .Register<Deliverable>(
                                resolver,
                                nameBuilder attr.Alias
                            )
                        |> ignore
                    )
            )
