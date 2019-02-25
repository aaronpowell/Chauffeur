module CompositionExtensions

open Umbraco.Core.Composing

type Composition with
    member this.RegisterAs<'TService, 'TTarget>() =
        this.Register(typeof<'TService>, typeof<'TTarget>)
