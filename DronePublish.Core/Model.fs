namespace DronePublish.Core

open System.IO
open FSharp.Json
open Elmish

type Model = {
    Conf: Conf
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
        File.ReadAllText file |> Json.deserialize<Model> |> Ok

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
                }
        (state, Cmd.none)
