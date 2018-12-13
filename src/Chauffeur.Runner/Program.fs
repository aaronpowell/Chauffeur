open System
open System.Reflection
open System.IO
open System.Threading

open Chauffeur.Host

let rec findSiteRoot path =
    if Directory.GetDirectoryRoot(path) = path then
        None
    else
        let files = Directory.EnumerateFiles(path, "*.config", SearchOption.TopDirectoryOnly)
                    |> Seq.filter (fun s -> s.EndsWith("Web.config", StringComparison.InvariantCultureIgnoreCase))
        match Seq.length files with
        | 0 -> Path.Combine(path, "..") |> findSiteRoot
        | 1 -> Some(path)
        | _ -> failwithf "Found more than 1 web.config at %s" path

let setData (domain : AppDomain) key value =
    domain.SetData(key, value)

[<EntryPoint>]
let main argv =
    match AppDomain.CurrentDomain.FriendlyName with
    | "chauffeur-domain" ->
        use host = new UmbracoHost(Console.In, Console.Out)
        let runner = host :> IChauffeurHost
        let _ = runner.Run() |> Async.AwaitTask |> Async.RunSynchronously

        0
    | _ ->
        let asm = Assembly.GetExecutingAssembly()
        let exePath = (new FileInfo(asm.Location)).Directory.FullName
        let siteRoot = findSiteRoot exePath

        match siteRoot with
        | Some siteRoot ->
            let webConfigPath = Path.Combine(siteRoot, "Web.config")
            let ads = new AppDomainSetup()
            ads.ConfigurationFile <- webConfigPath
            ads.ApplicationBase <- Path.Combine(exePath, "..")
            ads.PrivateBinPath <- exePath
            let domain = AppDomain.CreateDomain("chauffeur-domain",
                          AppDomain.CurrentDomain.Evidence,
                          ads)

            AppDomain.CurrentDomain.GetAssemblies()
            |> Array.iter (fun asm -> 
                            try
                                domain.Load(asm.FullName) |> ignore
                            with
                            | _ -> ignore())

            let setDomainData = setData domain
            let setThreadData = Thread.GetDomain() |> setData

            setDomainData "DataDirectory" (Path.Combine(siteRoot, "App_Data"))
            setDomainData ".appDomain" "From Domain"
            setDomainData ".appId" "From Domain"
            setDomainData ".appVPath" exePath
            setDomainData ".appPath" exePath

            setThreadData ".appDomain" "From Thread"
            setThreadData ".appId" "From Thread"
            setThreadData ".appVPath" exePath
            setThreadData ".appPath" exePath
            let thisAssembly = new FileInfo(asm.Location)
            let _ = domain.ExecuteAssembly(thisAssembly.FullName, argv)
            0

        | None ->
            printfn "Chauffeur was run from %s but we did not find Umbraco in any parents before hitting the root directory"
                    exePath
            -1
