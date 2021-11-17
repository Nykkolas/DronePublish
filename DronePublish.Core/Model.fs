namespace DronePublish.Core

open System.IO
open FSharp.Json
open Elmish

type ModelErrors =
    | CantDeserializeFile

type Model = {
    Conf: Conf
    DestDir: string
}

module Model =
    let saveStateToFile (state:Model) (file:string) =
        let saveFolder = Path.GetDirectoryName file
        if not (Directory.Exists saveFolder) then
            Directory.CreateDirectory saveFolder |> ignore

        let serializedState = Json.serialize state

        File.WriteAllText (file, serializedState)
        
        Ok ()
        //Error CantSaveState

    let saveState state =
        saveStateToFile state Const.saveFile

    let loadStateFromFile (file:string) =
        try 
            File.ReadAllText file |> Json.deserialize<Model> |> Ok
        with
            | ex -> Error CantDeserializeFile

    let loadState () =
        loadStateFromFile Const.saveFile

    let init () = 
        let stateResult = loadState ()
        let state = 
            match stateResult with
            | Ok s -> s
            | Error _ -> 
                {
                    Conf = { ExecutablesPath = "" }
                    DestDir = ""
                }
        (state, Cmd.none)
