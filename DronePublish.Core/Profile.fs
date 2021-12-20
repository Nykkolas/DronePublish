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
    let isSelected = function
        | Empty | NotSelected _ -> false
        | Selected _ -> true

    let select = function
        | Empty -> Empty
        | Selected p -> Selected p
        | NotSelected p -> Selected p

    let unSelect = function
        | Empty -> Empty
        | Selected p -> NotSelected p
        | NotSelected p -> NotSelected p

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

    let getProfileData = function
        | Empty -> None
        | Selected p -> Some p
        | NotSelected p -> Some p

    let removeAt index profiles = 
        profiles
        |> List.mapi (fun i el -> (i <> index, el))
        |> List.filter (fun (b, _) -> b = true)
        |> List.map (fun (b, e) -> e)

    let updateAt index value source =
        source
        |> List.mapi (fun i el -> if i = index then value else el)
