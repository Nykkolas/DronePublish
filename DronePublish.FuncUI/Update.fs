namespace DronePublish.FuncUI

open System
open System.Diagnostics
open System.IO
open Avalonia.Controls
open Elmish
open DronePublish.Core
open Xabe.FFmpeg
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation

type Msg =
    | ChooseExecutablesPath
    | ExecutablesPathChosen of string
    | ChooseSourceFile
    | SourceFileChosen of string array
    | GetSourceFileInfos
    | GotSourceFileInfos of Validation<IMediaInfo,ModelErrors>
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
            | _ -> ({ state with Conf = { state.Conf with ExecutablesPath = f } }, Cmd.batch [ Cmd.ofMsg SaveState; Cmd.ofMsg GetSourceFileInfos ])
        | ChooseSourceFile ->
            let dialog = Dialogs.getSourceFileDialog None state.SourceFile
            let showDialog window = dialog.ShowAsync (window) |> Async.AwaitTask
            (state, Cmd.OfAsync.perform showDialog window SourceFileChosen)
        | SourceFileChosen f ->
            match Array.length f with
            | 0 -> (state, Cmd.none)
            | _ -> ({ state with SourceFile = f.[0] }, Cmd.batch [ Cmd.ofMsg SaveState; Cmd.ofMsg GetSourceFileInfos ])
        | GetSourceFileInfos ->
            match state.SourceInfos with
            | Started -> (state, Cmd.none)
            | NotStarted | Resolved _  ->
                let createCmd p f =
                    FFmpeg.SetExecutablesPath p
                    ({ state with SourceInfos = Started}, Cmd.OfTask.perform FFmpeg.GetMediaInfo f (Ok >> GotSourceFileInfos))
                    
                let tryActualCmd =
                    createCmd
                    <!^> Model.validateExecutablePath state.Conf.ExecutablesPath
                    <*^> Model.validateSourceFile state.SourceFile
                
                tryActualCmd
                |> function 
                    | Ok c -> c
                    | Error e -> (state, Cmd.ofMsg (GotSourceFileInfos (Error e)))
                
        | GotSourceFileInfos i ->
            let mediaFileInfos =
                match i with
                | Ok media -> MediaFileInfos.createWithIMediaInfo media |> Ok
                | Error e -> Error e
            ({ state with SourceInfos = Resolved mediaFileInfos}, Cmd.ofMsg SaveState)
        | ChooseDestDir -> 
            let dialog = Dialogs.getFolderDialog "Répertoire de destination" state.DestDir
            let showDialog window = dialog.ShowAsync (window) |> Async.AwaitTask
            (state, Cmd.OfAsync.perform showDialog window DestDirChosen) 
        | DestDirChosen f ->
            match f with
            | "" -> (state, Cmd.none)
            | _ -> ({ state with DestDir = f }, Cmd.ofMsg SaveState)
        | SaveState ->
            Model.saveState state
            (state, Cmd.none)
        
