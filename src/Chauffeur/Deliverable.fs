namespace Chauffeur

open System.IO
open System.Threading.Tasks
open System
open FSharp.Control.Tasks.V2

[<AbstractClass>]
type Deliverable(reader : TextReader, writer : TextWriter) =
    member __.In = reader
    member __.Out = writer

    abstract member Run : string -> string[] -> Task<DeliverableResponse>

    default __.Run _ _ = task { return DeliverableResponse.Continue }

[<AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)>]
type DeliverableNameAttribute(name : string) =
    inherit Attribute()
    member __.Name = name

[<AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)>]
type DeliverableAliasAttribute(alias : string) =
    inherit Attribute()
    member __.Alias = alias