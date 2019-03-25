﻿namespace Chauffeur.Host

open System.Threading.Tasks
open Chauffeur

type IChauffeurHost =
    abstract member Run : unit -> Task<DeliverableResponse>
    abstract member RunWithArgs : string array -> Task<DeliverableResponse>
