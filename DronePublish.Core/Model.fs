namespace DronePublish.Core

open System.IO
open FSharp.Json
open Elmish
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open Xabe.FFmpeg

type ModelError =
    | CantDeserializeFile
    | CantFindFFMpegExe
    | CantFindSourceFile
    | CantFindDestDir

type Conf = {
    ExecutablesPath: string
}

type Model = {
    ConfFile: string
    Conf: Conf
    SourceFile: string
    SourceInfos: Deferred<Result<MediaFileInfos, ConversionError list>>
    DestDir: string
    Conversion: Deferred<Result<ConversionResult, ConversionError list>>
    Profiles: Profile list
}

module Model =
    let saveStateToFile (state:Model) (file:string) =
        let saveFolder = Path.GetDirectoryName file
        if not (Directory.Exists saveFolder) then
            Directory.CreateDirectory saveFolder |> ignore

        let serializedState = Json.serialize state

        File.WriteAllText (file, serializedState)

    let saveState state =
        saveStateToFile state state.ConfFile

    let loadStateFromFile (file:string) =
        try 
            File.ReadAllText file |> Json.deserialize<Model> |> Ok
        with
            | _ -> Error CantDeserializeFile

    let init confFile = 
        let stateResult = loadStateFromFile confFile
        let state = 
            match stateResult with
            | Ok s -> s
            | Error _ -> 
                {
                    ConfFile = confFile
                    Conf = { ExecutablesPath = @"./Ressources/bin" }
                    SourceFile = ""
                    SourceInfos = NotStarted
                    DestDir = ""
                    Conversion = NotStarted
                    Profiles = List.empty
                }
        (state, Cmd.none)
