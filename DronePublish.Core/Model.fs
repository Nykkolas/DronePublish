namespace DronePublish.Core

open System.IO
open FSharp.Json
open Elmish

type ModelError =
    | CantDeserializeFile
    | CantFindFFMpegExe
    | CantFindSourceFile
    | CantFindDestDir

type Conf = {
    ExecutablesPath: string
}

(* TODO : tester SourceFile ou DestDir avec une option (pour gérer le cas de l'initialisation + créer un type qui vérifie que le fichier existe) *)

type Model = {
    ConfFile: string
    Conf: Conf
    SourceFile: string
    SourceInfos: Deferred<Result<MediaFileInfos, ConversionError list>>
    DestDir: string
    Conversion: Deferred<Result<ConversionResult, ConversionError list>> // TO DELETE
    ConversionJobs: ConversionJobs
    Profiles: ProfilesCore.Model
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


    (* TODO : vider le log au démarrage *)
    let init confFile = 
        let stateResult = loadStateFromFile confFile
        let state = 
            match stateResult with
            | Ok s -> s
            | Error _ -> 
                {
                    ConfFile = confFile
                    Conf = { ExecutablesPath = @"./Resources/bin" }
                    SourceFile = ""
                    SourceInfos = NotStarted
                    DestDir = ""
                    Conversion = NotStarted
                    ConversionJobs = ConversionJobs.init ()
                    Profiles = ProfilesCore.init ()
                }
        (state, Cmd.none)
