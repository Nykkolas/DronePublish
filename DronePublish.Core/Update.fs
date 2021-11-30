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
    | GotSourceFileInfos of Validation<IMediaInfo,ConversionError>
    | ChooseDestDir
    | DestDirChosen of string
    | StartConvertion
    | ConvertionDone of Validation<IConversionResult,ConversionError>
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
                MediaFileInfos.tryReadIMediaInfo state.Conf.ExecutablesPath state.SourceFile
                |> function 
                    | Ok c -> 
                        { state with SourceInfos = Started}, 
                        Cmd.OfTask.perform (fun _ -> c) () (Ok >> GotSourceFileInfos)
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
            match state.Conversion with
            | Started -> (state, Cmd.none)
            | Resolved _ | NotStarted ->
                match Conversion.tryStart state.Conf.ExecutablesPath state.SourceFile state.DestDir @"output.mp4" with
                | Ok c -> 
                    ( { state with Conversion = Started }, Cmd.OfAsync.perform (fun _ -> c) () (Ok >> ConvertionDone))
                | Error e -> (state, Cmd.ofMsg (ConvertionDone (Error e)))
        
        | ConvertionDone r ->
            match r with
            | Error e -> ( { state with Conversion = Resolved (Error e) }, Cmd.none)
            | Ok c -> ( { state with Conversion = ConversionResult.create c |> Ok |> Resolved }, Cmd.ofMsg SaveState)
        
        | SaveState ->
            Model.saveState state
            (state, Cmd.none)

        
