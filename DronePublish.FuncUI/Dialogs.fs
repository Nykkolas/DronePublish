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

