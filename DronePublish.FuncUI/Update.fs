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
    | ChangeDestDir
    | Test

module Update =
    let update msg state window =
        match msg with
        | ChooseExecutablesPath -> 
            Trace.WriteLine "ChooseExecutablesPath"
            let dialog = OpenFolderDialog ()
            dialog.Title <- "Répertoire des binaires"
            dialog.Directory <- 
                if Directory.Exists state.Conf.ExecutablesPath then state.Conf.ExecutablesPath
                else Environment.GetFolderPath Environment.SpecialFolder.Personal
            let showDialog window = dialog.ShowAsync (window) |> Async.AwaitTask
            (state, Cmd.OfAsync.perform showDialog window ExecutablesPathChosen) 
        | ExecutablesPathChosen f ->
            let newState = { state with Conf = { state.Conf with ExecutablesPath = f } }
            Model.saveState newState |> ignore
            (newState, Cmd.none)
        | ChangeDestDir -> 
            Trace.WriteLine "ChangeDestDir"
            (state, Cmd.none)
        | Test -> 
            Trace.WriteLine "Le test est Ok"
            (state, Cmd.none)
        
