namespace DronePublish.FuncUI

open Avalonia.Controls
open Avalonia.Threading
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish
open Elmish


(* TODO : Rendre joli : plus petit + texte centré + marges + bouton OK en bas à droite, pas de déco *)

module InfoDialogView =
    type State = {
        InfoDialog: HostWindow
        Message: string
    }

    type Msg =
        | Close

    let init (infoDialog, message) = 
        {
            InfoDialog = infoDialog
            Message = message
        }

    let update msg state =
        match msg with
        | Close -> 
            state.InfoDialog.Close ()
            state

    let view state dispatch =
        StackPanel.create [
            StackPanel.children [
                TextBlock.create [ TextBlock.text state.Message ]
                Button.create [
                    Button.content "Ok"
                    Button.onClick (fun _ -> dispatch Close)
                ]
            ]
        
        ]

    type InfoDialog (title, message) as this =
        inherit HostWindow ()

        do
            base.Title <- title
            base.Width <- 400.0
            base.Height <- 200.0
            base.WindowStartupLocation <- WindowStartupLocation.CenterOwner

            Elmish.Program.mkSimple init update view
            |> Program.withHost this
            |> Program.runWith (this, message)

    let show (window:Window) title message =
        Dispatcher.UIThread.InvokeAsync<unit>(fun _ ->
            let infoDialog = InfoDialog (title, message)
            infoDialog.ShowDialog<unit> window)
        |> Async.AwaitTask
