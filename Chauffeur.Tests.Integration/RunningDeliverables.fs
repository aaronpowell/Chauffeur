module ``Running Deliverables``

open Chauffeur
open TestHelpers
open Xunit
open System.IO
open FsUnit.Xunit
open System.Data.SqlServerCe
open System.Data

let setupDelivery deliverableName steps dbLocation =
    let chauffeurFolder = getChauffeurFolder dbLocation
    File.AppendAllText(Path.Combine([| chauffeurFolder.FullName; deliverableName |]), steps)

let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

let getTrackedDeliveries() =
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
        setupInstallDelivery x.DatabaseLocation
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Tracks the delivery in the database``() =
        setupInstallDelivery x.DatabaseLocation
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            let! dbSet = getTrackedDeliveries()
            let rows = dbSet.Tables.["Chauffeur_Delivery"].Rows
            rows.Count |> should equal 1
            let row = rows.[0]
            row.["Name"] :?> string |> should equal "001-install.delivery"
            row.["SignedFor"] :?> bool |> should be True
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Won't re-run the delivery if it was previously run``() =
        setupInstallDelivery x.DatabaseLocation
        async {
            let! response1 = [| "delivery" |]
                             |> x.Host.Run
                             |> Async.AwaitTask
            let! response2 = [| "delivery" |]
                             |> x.Host.Run
                             |> Async.AwaitTask
            response2 |> should equal DeliverableResponse.Continue
            let! dbSet = getTrackedDeliveries()
            let rows = dbSet.Tables.["Chauffeur_Delivery"].Rows
            rows.Count |> should equal 1
        }
        |> Async.RunSynchronously

type ``Multi-step delivery``() =
    inherit UmbracoHostTestBase()
    [<Fact>]
    member x.``Can run a delivery with multiple steps``() =
        setupDelivery "some.delivery" (sprintf "install y%sct get-all" System.Environment.NewLine) x.DatabaseLocation
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
        setupInstallDelivery x.DatabaseLocation
        setupDelivery "002-get-doctypes.delivery" "ct get-all" x.DatabaseLocation
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Multiple deliveriers are tracked individually``() =
        setupInstallDelivery x.DatabaseLocation
        setupDelivery "002-get-doctypes.delivery" "ct get-all" x.DatabaseLocation
        async {
            let! response = [| "delivery" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            let! dbSet = getTrackedDeliveries()
            let rows = dbSet.Tables.["Chauffeur_Delivery"].Rows
            rows.Count |> should equal 2
            let asserter deliveryName (row : DataRow) =
                row.["Name"] :?> string |> should equal deliveryName
                row.["SignedFor"] :?> bool |> should be True
            rows.[0] |> asserter "001-install.delivery"
            rows.[1] |> asserter "002-get-doctypes.delivery"
        }
        |> Async.RunSynchronously
