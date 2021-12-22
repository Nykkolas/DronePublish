#r "nuget: FParsec"

open FParsec

type ParsableBitrate = ParsableBitrate of string
type PositiveLong = PositiveLong of int64

type Bitrate = {
    UserInput: ParsableBitrate
    Bitrate: PositiveLong
}

module Bitrate =
    let parser = pint64 .>>. (choice [pchar 'k' >>% 1000L; pchar 'M' >>% 1000000L; eof >>% 1L])
    
    let tryCreate str =
        match run parser str with
        | Failure (e, _, _) -> Result.Error e
        | Success ((l, u), _, _) -> 
            Result.Ok { UserInput = ParsableBitrate str; Bitrate = PositiveLong (l * u) }
           
    let tryConvert str =
        match run parser str with
        | Failure (e, _, _) -> Result.Error e
        | Success ((l, u), _, _) -> Result.Ok (l * u)

    let unWrapBitrate b =
        let (PositiveLong bitrate) = b.Bitrate
        bitrate

    let unWrapUserInput b =
        let (ParsableBitrate userInput) = b.UserInput
        userInput

module Proto_Parser =
    let resultOk1 = Bitrate.tryCreate "123M"
    let resultOk2 = Bitrate.tryCreate "123k"
    let resultOk3 = Bitrate.tryCreate "123"
    let resultKo1 = Bitrate.tryCreate "1 23M"    

    printfn "%A" [ resultOk1; resultOk2; resultOk3; resultKo1 ]
