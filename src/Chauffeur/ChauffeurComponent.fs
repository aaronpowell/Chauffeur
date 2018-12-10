namespace Chauffeur

open Umbraco.Core.Components

type ChauffeurComponent() =
    inherit UmbracoComponentBase()

    override __.Compose(composition) =
        composition.Container.Register<IChauffeurSettings, ChauffeurSettings>() |> ignore