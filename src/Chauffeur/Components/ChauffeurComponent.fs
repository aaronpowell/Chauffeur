namespace Chauffeur.Components

open Umbraco.Core.Composing
open Chauffeur
open CompositionExtensions
open Packaging

[<RuntimeLevelAttribute(MinLevel = Umbraco.Core.RuntimeLevel.BootFailed)>]
type ChauffeurComponent() =
    interface IComposer with
        member __.Compose(register) =
            register.RegisterAs<IChauffeurSettings, ChauffeurSettings>()
            register.RegisterAs<ISqlCeFactory, SqlCeFactory>()
            register.RegisterAs<IPackageInstallWrapper, PackageImportWrapper>()