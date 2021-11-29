namespace DronePublish.FuncUI

open Elmish
open Avalonia.Controls
open Avalonia.Media
open Avalonia.Layout
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish
open FsToolkit.ErrorHandling
open DronePublish.Core

module ConvertionView =
    let view state dispatch =
        let convertionReadyness = Model.validateConvertionReadiness state.Conf.ExecutablesPath state.SourceFile state.DestDir
        
        DockPanel.create [
            DockPanel.dock Dock.Top
            DockPanel.margin 10.0
            DockPanel.children [
                DockPanel.create [
                    DockPanel.dock Dock.Top
                    DockPanel.children [
                        Button.create [
                            Button.dock Dock.Right
                            Button.verticalAlignment VerticalAlignment.Center
                            Button.isEnabled (
                                convertionReadyness
                                |> Result.either (fun _ -> true) (fun _ -> false)
                            )
                            Button.content "Convertir"
                            Button.onClick (fun _ -> StartConvertion |> dispatch)
                        ]
                        TextBlock.create [
                            TextBlock.dock Dock.Right
                            TextBlock.verticalAlignment VerticalAlignment.Center
                            TextBlock.margin (horizontal = 10.0, vertical = 0.0)
                            TextBlock.textAlignment TextAlignment.Right
                            TextBlock.text (
                                convertionReadyness
                                |> Result.either 
                                    (fun _ -> "Prêt !") 
                                    (fun _ -> "Pas prêt !")                                    
                            )
                        ]
                    ]                
                ]
                Expander.create [
                    Expander.dock Dock.Top
                    Expander.header "Logs de conversion"
                    Expander.content (
                        TextBox.create [ 
                            TextBox.textWrapping TextWrapping.Wrap
                            TextBox.isReadOnly true
                            TextBox.text (
                                convertionReadyness
                                |> Result.either 
                                    (fun _ ->  "Pas d'erreur : .\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n." )
                                    (fun e -> sprintf "Erreurs : \n%A" e)
                            )
                        ]
                    )   
                ]
                
            ]
        ]
        
(* EXEMPLE DE FENETRE MODALE cf ConvertionView.fs/IDialogs.fs/Dialogs.fs/Program.fs/DialogTest.fs *)    
//type ConvertionWindow () as this =
//    inherit HostWindow ()

//    do
//        base.Title <- "Convertion en cours..."
//        base.Width <- 400.0
//        base.Height <- 400.0

//        Elmish.Program.mkSimple (fun _ -> ()) (fun _  _ -> ()) ConvertionView.view
//        |> Program.withHost this
//        |> Program.run


