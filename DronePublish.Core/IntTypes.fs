namespace DronePublish.Core

open System

type IntError =
    | IsLowerOrEqualThanZero
    | ErrorParsingText

type PositiveInt = PositiveInt of int

type PositiveLong = PositiveLong of int64

module PositiveInt =
    let tryCreate i =
        if i <= 0 then
            Error IsLowerOrEqualThanZero
        else
            PositiveInt i |> Ok

    let tryParse (text:string) =
        try
            Convert.ToInt32 (text) 
            |> tryCreate
        with
            | _ -> Error ErrorParsingText

    let unWrap positiveInt =
        let (PositiveInt s) = positiveInt
        s

    let toString positiveInt =
        let (PositiveInt s) = positiveInt
        s.ToString ()

module PositiveLong =
    let tryCreate i =
        if i <= 0L then
            Error IsLowerOrEqualThanZero
        else
            PositiveLong i |> Ok
    
    let tryParse (text:string) =
        try
            Convert.ToInt64 (text) 
            |> tryCreate
        with
            | _ -> Error ErrorParsingText

    let unWrap positiveLong =
        let (PositiveLong s) = positiveLong
        s

    let toString positiveLong =
        let (PositiveLong s) = positiveLong
        s.ToString ()
