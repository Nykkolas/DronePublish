namespace DronePublish.FuncUI

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish
open Elmish

(* TODO :  bouton OK, pas de déco *)

module InfoDialogView =
    let init message = message

    let view state dispatch =
        TextBlock.create [ TextBlock.text state ]

type InfoDialog (title, message) as this =
    inherit HostWindow ()

    do
        base.Title <- title
        base.Width <- 400.0
        base.Height <- 200.0
        base.WindowStartupLocation <- WindowStartupLocation.CenterOwner

        Elmish.Program.mkSimple InfoDialogView.init (fun _  _ -> message) InfoDialogView.view
        |> Program.withHost this
        |> Program.runWith message
