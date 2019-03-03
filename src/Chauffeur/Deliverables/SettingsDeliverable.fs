namespace Chauffeur.Deliverables

open Chauffeur
open System.Collections.Generic
open FSharp.Control.Tasks.V2

type Row = { Setting: string; Value: string }

[<DeliverableName("settings")>]
type SettingsDeliverable(reader, writer, settings : IChauffeurSettings) =
    inherit Deliverable(reader, writer)

    override __.Run _ _ =
        let dic = new Dictionary<string, string>()

        dic.Add("Umbraco Version", settings.UmbracoVersion)
        dic.Add("Chauffeur Version", settings.ChauffeurVersion)

        dic.Add("Site Root", match settings.TryGetSiteRootDirectory() with
                             | true, path -> path
                             | false, _ -> "Failed to access")

        dic.Add("Umbraco Directory", match settings.TryGetUmbracoDirectory() with
                                     | true, path -> path
                                     | false, _ -> "Failed to access")

        dic.Add("Chauffeur Directory", match settings.TryGetChauffeurDirectory() with
                                       | true, path -> path
                                       | false, _ -> "Failed to access")

        dic.Add("Connection String", settings.ConnectionString.ConnectionString)

        let rows = dic
                   |> Seq.map(fun d -> { Setting = d.Key; Value = dic.[d.Key] })

        task {
            do! TextWriterExtensions.WriteTableAsync writer rows (Set.empty |> dict)
            return DeliverableResponse.Continue
        }

