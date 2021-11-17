namespace DronePublish.Core

open Elmish
open Elmish.WPF
open System.Diagnostics

type WpfMsg =
    | WpfChooseExecutablesPath
    | WpfChangeDestDir
    | WpfTest

module WpfUpdate =
    let update msg m =
        match msg with
        | WpfChooseExecutablesPath -> 
            Trace.WriteLine "ChooseExecutablesPath"
            (m, Cmd.none)
        | WpfChangeDestDir -> 
            Trace.WriteLine "ChangeDestDir"
            (m, Cmd.none) 
        | WpfTest -> 
            Trace.WriteLine "Le test est Ok"
            (m, Cmd.none)
        
    // Spécifique à Elmish.WPF
    let bindings () =
        [
            "ExecutablesPath" |> Binding.oneWay (fun m -> m.Conf.ExecutablesPath)
            "ChangeExecutablesPath" |> Binding.cmd (fun _ -> WpfChooseExecutablesPath)
            //"DestDir" |> Binding.oneWay (fun m -> m.Conf.DestDir)
            "ChangeDestDir" |> Binding.cmd (fun _ -> WpfChangeDestDir)
        ]
        
    let main window =
        WpfProgram.mkProgram Model.init update bindings
        |> WpfProgram.startElmishLoop window
