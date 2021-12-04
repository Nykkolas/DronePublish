namespace DronePublish.Core

open Xabe.FFmpeg

type Codec =
    | H264
    | H264_NVENC

type ProfileData = {
    Nom: NonEmptyString100
    Suffixe: NonEmptyString100
    Bitrate: PositiveLong
    Width: PositiveInt
    Height: PositiveInt
    Codec: Codec
}

type Profile =
    | Empty
    | Selected of ProfileData
    | NotSelected of ProfileData

module Profile =
    let createData nom suffixe bitrate width height codec =
        {
            Nom = nom
            Suffixe = suffixe
            Bitrate = bitrate
            Width = width
            Height = height
            Codec = codec
        }

    let convertCodec = function
        | H264 -> VideoCodec.h264
        | H264_NVENC -> VideoCodec.h264_nvenc

    let getProfileData profile =
        match profile with
        | Empty -> None
        | Selected p -> Some p
        | NotSelected p -> Some p

