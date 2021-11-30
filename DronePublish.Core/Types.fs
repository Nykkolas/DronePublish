namespace DronePublish.Core

open Xabe.FFmpeg

type Conf = {
    ExecutablesPath: string
}

type Codec =
    | H264

type ProfileData = {
    Nom: string
    Suffixe: string
    Bitrate: int64
    Width: int
    Height: int
    Codec: Codec
}

type Profile =
    | SELECTED of ProfileData
    | NOTSELECTED of ProfileData

type Profiles = Profile array

module Profiles =
    let convertCodec = function
        | H264 -> VideoCodec.h264

    let selectedProfiles profiles =
        profiles
        |> Array.filter (
            function 
            | SELECTED _ -> true
            | NOTSELECTED _ -> false
        )
