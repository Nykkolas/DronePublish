namespace DronePublish.FuncUI

open System
open Avalonia.Controls

module Dialogs =
    let getFolderDialog =
        let dialog = OpenFolderDialog()
        dialog.Directory <- Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
        dialog.Title <- "Choose where to look up for music"
        dialog

