module ServiceContainerExtensions

open LightInject

type ServiceContainer with
    member this.TryGetInstance<'T>( name) =
        match this.CanGetInstance(typeof<'T>, name) with
        | true -> Some(this.GetInstance<'T>(name))
        | false -> None