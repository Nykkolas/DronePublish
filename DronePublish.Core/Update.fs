namespace DronePublish.Core

open System
open System.Diagnostics
open System.IO
open Elmish
open Xabe.FFmpeg
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation

type Msg =
    | ChooseExecutablesPath
    | ExecutablesPathChosen of string
    | ChooseSourceFile
    | SourceFileChosen of string array
    | GetSourceFileInfos
    | GotSourceFileInfos of Validation<IMediaInfo,ModelError>
    | ChooseDestDir
    | DestDirChosen of string
    | StartConvertion
    | ConvertionDone of unit
    | SaveState

module Update =
    let update msg state dialogs =
        match msg with
        | ChooseExecutablesPath -> 
            (state, Cmd.OfAsync.perform dialogs.ShowFolderDialog ("Répertoire des binaires", state.Conf.ExecutablesPath) ExecutablesPathChosen)
        | ExecutablesPathChosen f ->
            match f with
            | "" -> (state, Cmd.none)
            | _ -> ({ state with Conf = { state.Conf with ExecutablesPath = f } }, Cmd.batch [ Cmd.ofMsg SaveState; Cmd.ofMsg GetSourceFileInfos ])
        | ChooseSourceFile ->
            (state, Cmd.OfAsync.perform dialogs.ShowSourceFileDialog state.SourceFile SourceFileChosen)
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
            (state, Cmd.OfAsync.perform dialogs.ShowFolderDialog ("Répertoire de destination", state.DestDir) DestDirChosen)
        | DestDirChosen f ->
            match f with
            | "" -> (state, Cmd.none)
            | _ -> ({ state with DestDir = f }, Cmd.ofMsg SaveState)
        | StartConvertion ->
            (state, Cmd.OfAsync.perform dialogs.ShowConvertionWindow () ConvertionDone)
        | ConvertionDone u ->
            (state, Cmd.none)
        | SaveState ->
            Model.saveState state
            (state, Cmd.none)

        
