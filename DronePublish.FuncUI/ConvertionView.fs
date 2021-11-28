namespace DronePublish.FuncUI

open Avalonia.Controls
open Elmish
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish

module ConvertionView =
    let init () =
        ()
        
    let update msg state =
        ()

    let view state dispatch =
        TextBlock.create [
            TextBlock.text "Le suivi ici"
        ]

    
type ConvertionWindow () as this =
    inherit HostWindow ()

    do
        base.Title <- "Convertion en cours..."
        base.Width <- 400.0
        base.Height <- 400.0

        Elmish.Program.mkSimple ConvertionView.init ConvertionView.update ConvertionView.view
        |> Program.withHost this
        |> Program.run


