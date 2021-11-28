namespace DronePublish.FuncUI

open System
open System.IO
open Avalonia.Controls
open Avalonia.Threading
open Avalonia.FuncUI.Components.Hosts

module Dialogs =
    let getFolderDialog title directory =
        let dialog = OpenFolderDialog ()
        dialog.Title <- title 
        dialog.Directory <- 
            if Directory.Exists directory then directory
            else Environment.GetFolderPath Environment.SpecialFolder.Personal
        dialog

    let showFolderDialog window (title, directory) =
        let dialog = getFolderDialog title directory
        dialog.ShowAsync (window) |> Async.AwaitTask

    let getSourceFileDialog (filters: FileDialogFilter seq option) (currentSourceFile:string) =
        let dialog = OpenFileDialog()

        let filters =
            match filters with
            | Some filter -> filter
            | None ->
                let filter = FileDialogFilter()
                filter.Extensions <-
                    Collections.Generic.List
                        (seq {
                            "mov"
                            "mp4" })
                filter.Name <- "Vidéo"
                seq { filter }

        dialog.AllowMultiple <- false
        dialog.Directory <- Path.GetDirectoryName currentSourceFile
        dialog.Title <- "Sélectionner le fichier à convertir"
        dialog.Filters <- System.Collections.Generic.List(filters)
        dialog

    let showSourceFileDialog window title =
        let dialog = getSourceFileDialog None title
        dialog.ShowAsync (window) |> Async.AwaitTask

    let showConvertionWindow (window:Window) () =
        Dispatcher.UIThread.InvokeAsync<unit>(fun _ ->
            let convertionWindow = ConvertionWindow()
            convertionWindow.ShowDialog<unit> window)
        |> Async.AwaitTask


    