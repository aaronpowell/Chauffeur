namespace Chauffeur.Components

open Umbraco.Core.Components
open Umbraco.Core.Composing
open Chauffeur

[<RuntimeLevelAttribute(MinLevel = Umbraco.Core.RuntimeLevel.BootFailed)>]
type ChauffeurComponent() =
    interface IComposer with
        member __.Compose(register) =
            register.Register<IChauffeurSettings, ChauffeurSettings>()
            register.Register<ISqlCeFactory, SqlCeFactory>()