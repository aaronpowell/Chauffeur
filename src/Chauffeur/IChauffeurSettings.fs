namespace Chauffeur

open System.Configuration

type IChauffeurSettings =
    abstract member ConnectionString : ConnectionStringSettings


type internal ChauffeurSettings() =
    interface IChauffeurSettings with
        member __.ConnectionString = ConfigurationManager.ConnectionStrings.["umbracoDbDSN"]