namespace Chauffeur

open System
open Umbraco.Web

type NullHttpContextAccessor() =
    interface IHttpContextAccessor with
        member __.HttpContext
            with get (): Web.HttpContext = null
            and set (_: Web.HttpContext): unit = 
                raise (System.NotImplementedException())