module ``Running Deliverables``

open Chauffeur
open Xunit
open System.IO
open FsUnit.Xunit
open System.Data.SqlServerCe
open System.Data
open System
open Chauffeur.TestingTools

let setupDelivery deliverableName steps (chauffeurFolder: DirectoryInfo) =
    File.AppendAllText(Path.Combine([| chauffeurFolder.FullName; deliverableName |]), steps)

let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

let trackedDeliveries =
    async {
        use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
        let cmd = new SqlCeCommand("select * from Chauffeur_Delivery", connection)
        connection.Open()
        let! reader = cmd.ExecuteReaderAsync() |> Async.AwaitTask
        let dbSet = new System.Data.DataSet()
        dbSet.Load(reader, System.Data.LoadOption.OverwriteChanges, [| "Chauffeur_Delivery" |])
        return dbSet
    }

type ``Working with a fresh install``() =
    inherit UmbracoHostTestBase()
    let setupInstallDelivery = setupDelivery "001-install.delivery" "install y"

    [<Fact>]
    member x.``Can install an instance with a Deliverable``() =
        x.GetChauffeurFolder() |> setupInstallDelivery
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Tracks the delivery in the database``() =
        x.GetChauffeurFolder() |> setupInstallDelivery
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            let! dbSet = trackedDeliveries
            let rows = dbSet.Tables.["Chauffeur_Delivery"].Rows
            rows.Count |> should equal 1
            let row = rows.[0]
            row.["Name"] :?> string |> should equal "001-install.delivery"
            row.["SignedFor"] :?> bool |> should be True
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Won't re-run the delivery if it was previously run``() =
        x.GetChauffeurFolder() |> setupInstallDelivery
        async {
            let! response1 = [| "delivery" |]
                             |> x.Host.Run
                             |> Async.AwaitTask
            let! response2 = [| "delivery" |]
                             |> x.Host.Run
                             |> Async.AwaitTask
            response2 |> should equal DeliverableResponse.Continue
            let! dbSet = trackedDeliveries
            let rows = dbSet.Tables.["Chauffeur_Delivery"].Rows
            rows.Count |> should equal 1
        }
        |> Async.RunSynchronously

type ``Multi-step delivery``() =
    inherit UmbracoHostTestBase()
    [<Fact>]
    member x.``Can run a delivery with multiple steps``() =
        x.GetChauffeurFolder()
        |> setupDelivery "some.delivery" (sprintf "install y%sct get-all" System.Environment.NewLine)
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }

    [<Fact>]
    member x.``Can run a delivery with multiple steps including comments``() =
        x.GetChauffeurFolder()
        |> setupDelivery "some.delivery" (sprintf "install y%s## this is a comment%sct get-all" Environment.NewLine Environment.NewLine)
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }

type ``Multiple deliveries``() =
    inherit UmbracoHostTestBase()
    let setupInstallDelivery = setupDelivery "001-install.delivery" "install y"

    [<Fact>]
    member x.``Can run multiple deliveries at once``() =
        x.GetChauffeurFolder() |> setupInstallDelivery
        x.GetChauffeurFolder() |>setupDelivery "002-get-doctypes.delivery" "ct get-all"
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Multiple deliveriers are tracked individually``() =
        x.GetChauffeurFolder() |> setupInstallDelivery
        x.GetChauffeurFolder() |> setupDelivery "002-get-doctypes.delivery" "ct get-all"
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            let! dbSet = trackedDeliveries
            let rows = dbSet.Tables.["Chauffeur_Delivery"].Rows
            rows.Count |> should equal 2
            let asserter deliveryName (row : DataRow) =
                row.["Name"] :?> string |> should equal deliveryName
                row.["SignedFor"] :?> bool |> should be True
            rows.[0] |> asserter "001-install.delivery"
            rows.[1] |> asserter "002-get-doctypes.delivery"
        }
        |> Async.RunSynchronously

type ``Deliveries with parameters``() =
    inherit UmbracoHostTestBase()
    let setupInstallDelivery = setupDelivery "001-install.delivery" "install $Install$"

    [<Fact>]
    member x.``When passing $Install flag it will be sustituted and used``() =
        x.GetChauffeurFolder() |> setupInstallDelivery
        async {
            let! response = [| "delivery -p:Install=y" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            let! dbSet = trackedDeliveries
            let rows = dbSet.Tables.["Chauffeur_Delivery"].Rows
            rows.Count |> should equal 1
            let asserter deliveryName (row : DataRow) =
                row.["Name"] :?> string |> should equal deliveryName
                row.["SignedFor"] :?> bool |> should be True
            rows.[0] |> asserter "001-install.delivery"
        }
        |> Async.RunSynchronously