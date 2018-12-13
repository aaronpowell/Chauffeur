namespace Chauffeur.Components

open Umbraco.Core.Components
open Chauffeur

type ChauffeurComponent() =
    inherit UmbracoComponentBase()

    override __.Compose(composition) =
        composition.Container.Register<IChauffeurSettings, ChauffeurSettings>() |> ignore
        composition.Container.Register<ISqlCeFactory, SqlCeFactory>() |> ignore