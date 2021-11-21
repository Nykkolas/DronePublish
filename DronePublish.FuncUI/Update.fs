namespace DronePublish.FuncUI

open System
open System.Diagnostics
open System.IO
open Avalonia.Controls
open Elmish
open DronePublish.Core

type Msg =
    | ChooseExecutablesPath
    | ExecutablesPathChosen of string
    | ChooseDestDir
    | DestDirChosen of string
    | SaveState

module Update =
    let update msg state window =
        match msg with
        | ChooseExecutablesPath -> 
            let dialog = Dialogs.getFolderDialog "Répertoire des binaires" state.Conf.ExecutablesPath
            let showDialog window = dialog.ShowAsync (window) |> Async.AwaitTask
            (state, Cmd.OfAsync.perform showDialog window ExecutablesPathChosen) 
        | ExecutablesPathChosen f ->
            match f with
            | "" -> (state, Cmd.none)
            | _ -> ({ state with Conf = { state.Conf with ExecutablesPath = f } }, Cmd.ofMsg SaveState)
        | ChooseDestDir -> 
            let dialog = Dialogs.getFolderDialog "Répertoire de destination" state.DestDir
            let showDialog window = dialog.ShowAsync (window) |> Async.AwaitTask
            (state, Cmd.OfAsync.perform showDialog window DestDirChosen) 
        | DestDirChosen f ->
            match f with
            | "" -> (state, Cmd.none)
            | _ -> ({ state with DestDir = f }, Cmd.ofMsg SaveState)
        | SaveState ->
            Model.saveState state |> ignore
            (state, Cmd.none)
        
