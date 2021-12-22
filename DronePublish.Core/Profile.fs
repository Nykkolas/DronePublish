namespace DronePublish.Core

open Xabe.FFmpeg
open FParsec

type Codec =
    | H264
    | H264_NVENC

type ProfileError = 
    | CantParseBitrate of string
    | PositiveLongBitrateError of IntError

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

    (* TODO : Autoriser 8.2M *)
    let tryParseBitrate str =
        let parser = pint64 .>>. (choice [pchar 'k' >>% 1000L; pchar 'M' >>% 1000000L; eof >>% 1L])

        match run parser str with
        | Failure (e, _, _) -> Result.Error (CantParseBitrate e)
        | Success ((l, u), _, _) -> PositiveLong.tryCreate (l * u) |> Result.mapError PositiveLongBitrateError
