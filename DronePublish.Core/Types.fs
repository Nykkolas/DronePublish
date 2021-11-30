namespace DronePublish.Core

open System.IO

type Conf = {
    ExecutablesPath: string
}

type Codec =
    | H264

type ProfileData = {
    Nom: string
    Suffixe: string
    Bitrate: int64
    Width: uint
    Height: uint
    Codec: Codec
}

type Profile =
    | SELECTED of ProfileData
    | NOTSELECTED of ProfileData

type Profiles = Profile array

module Profiles =
    let selectedProfiles profiles =
        profiles
        |> Array.filter (
            function 
            | SELECTED _ -> true
            | NOTSELECTED _ -> false
        )
