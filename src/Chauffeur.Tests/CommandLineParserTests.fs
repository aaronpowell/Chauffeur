namespace Chauffeur.Tests

open Xunit
open CommandLineParser
open FsUnit.Xunit
open System.Collections.Generic

module CommandLineParserTests =
    type DataSource() =
        static let data = [|
                        [| "help" :> obj; [ "help" ] :> obj |];
                        [| "help user" :> obj; [ "help"; "user" ] :> obj |]
                        [| "user change-password admin P@55-w0rd!1" :> obj; [ "user"; "change-password"; "admin"; "P@55-w0rd!1" ] :> obj |]
                        [| "user change-name admin \"Aaron Powell\"" :> obj; [ "user"; "change-name"; "admin"; "Aaron Powell" ] :> obj |]
                   |]

        static member TestData =
            data :> IEnumerable<obj array>

    [<Theory>]
    [<MemberData("TestData", MemberType = typeof<DataSource>)>]
    let ``can parse values as expected`` input expected =
        let result = parseCommandLine input
        result |> should equal expected
