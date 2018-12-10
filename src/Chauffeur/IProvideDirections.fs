namespace Chauffeur

open System.Threading.Tasks

type IProvideDirections =
    abstract Directions : unit -> Task

