namespace Chauffeur

open System.Data.SqlServerCe

type ISqlCeFactory =
    abstract member CreateDatabase : string -> unit

type SqlCeFactory() =
    interface ISqlCeFactory with
        member __.CreateDatabase(connStr: string): unit = 
            let engine = new SqlCeEngine(connStr)
            engine.CreateDatabase()
        
    

