namespace DronePublish.Core

open System.IO
open FSharp.Json
open Elmish
open FsToolkit.ErrorHandling

type ModelError =
    | CantDeserializeFile
    | CantFindFFMpegExe
    | CantFindSourceFile
    | CantFindDestDir

type Deferred<'a> =
    | NotStarted
    | Started
    | Resolved of 'a

type Model = {
    ConfFile: string
    Conf: Conf
    SourceFile: string
    SourceInfos: Deferred<Validation<MediaFileInfos, ModelError>>
    DestDir: string
}

module Model =
    let validateExecutablePath path =
        if (File.Exists (Path.Combine (path, "ffmpeg.exe"))) then
            Ok path
        else
            Error CantFindFFMpegExe

    let validateSourceFile file =
        if (File.Exists file) then
            Ok file
        else
            Error CantFindSourceFile

    let validateDestDir destDir =
        if (Directory.Exists destDir) then
            Ok destDir
        else
            Error CantFindDestDir

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
                    Conf = { ExecutablesPath = "" }
                    SourceFile = ""
                    SourceInfos = NotStarted
                    DestDir = ""
                }
        (state, Cmd.none)
