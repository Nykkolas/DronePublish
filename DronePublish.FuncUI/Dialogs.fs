namespace DronePublish.FuncUI

open System
open System.IO
open Avalonia.Controls

module Dialogs =
    let getFolderDialog title directory =
        let dialog = OpenFolderDialog ()
        dialog.Title <- title 
        dialog.Directory <- 
            if Directory.Exists directory then directory
            else Environment.GetFolderPath Environment.SpecialFolder.Personal
        dialog

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