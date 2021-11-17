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
    | Test

module Update =
    let update msg state window =
        match msg with
        | ChooseExecutablesPath -> 
            let dialog = Dialogs.getFolderDialog "Répertoire des binaires" state.Conf.ExecutablesPath
            let showDialog window = dialog.ShowAsync (window) |> Async.AwaitTask
            (state, Cmd.OfAsync.perform showDialog window ExecutablesPathChosen) 
        | ExecutablesPathChosen f ->
            let newState = { state with Conf = { state.Conf with ExecutablesPath = f } }
            Model.saveState newState |> ignore
            (newState, Cmd.none)
        | ChooseDestDir -> 
            let dialog = Dialogs.getFolderDialog "Répertoire de destination" state.DestDir
            let showDialog window = dialog.ShowAsync (window) |> Async.AwaitTask
            (state, Cmd.OfAsync.perform showDialog window DestDirChosen) 
        | DestDirChosen f ->
            let newState = { state with DestDir = f }
            Model.saveState newState |> ignore
            (newState, Cmd.none)            
        | Test -> 
            Trace.WriteLine "Le test est Ok"
            (state, Cmd.none)
        
