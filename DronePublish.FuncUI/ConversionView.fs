namespace DronePublish.FuncUI

open Elmish
open Avalonia.Controls
open Avalonia.Media
open Avalonia.Layout
open Avalonia.FuncUI.DSL
open FsToolkit.ErrorHandling
open DronePublish.Core

(* TODO : afficher le nom du profile échoué dans le log (plutôt que le numéro) *)

module ConversionView =
    let view state dispatch =
        let convertionReadyness = Conversion.validateReadiness state.Conf.ExecutablesPath state.SourceFile state.DestDir

        DockPanel.create [
            DockPanel.dock Dock.Top
            DockPanel.margin 10.0
            DockPanel.children [
                DockPanel.create [
                    DockPanel.dock Dock.Top
                    DockPanel.children [
                        //Button.create [
                        //    Button.dock Dock.Right
                        //    Button.verticalAlignment VerticalAlignment.Center
                        //    Button.isEnabled (
                        //        convertionReadyness
                        //        |> Result.either (fun _ -> true) (fun _ -> false)
                        //    )
                        //    Button.content "Convertir"
                        //    Button.onClick (fun _ -> StartConversion |> dispatch)
                        //]

                        Button.create [
                            Button.dock Dock.Right
                            Button.verticalAlignment VerticalAlignment.Center
                            Button.isEnabled (
                                convertionReadyness
                                |> Result.either (fun _ -> true) (fun _ -> false)
                            )
                            Button.content "Convertir"
                            Button.onClick (fun _ -> StartConversionJobs |> dispatch)
                        ]

                        TextBlock.create [
                            TextBlock.dock Dock.Right
                            TextBlock.verticalAlignment VerticalAlignment.Center
                            TextBlock.margin (horizontal = 10.0, vertical = 0.0)
                            TextBlock.textAlignment TextAlignment.Right
                            TextBlock.fontWeight FontWeight.Bold
                            match convertionReadyness with
                            | Ok _ ->
                                TextBlock.foreground "Green"
                                TextBlock.text "Prêt !"
                            | Error _ -> 
                                TextBlock.foreground "Red"
                                TextBlock.text "Pas prêt !"
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
                                |> function 
                                    | Error e -> sprintf "Erreurs à résoudre avant de lancer : \n%A" e
                                    | Ok _ -> state.ConversionJobs.Log           
                            )
                        ]
                    )   
                ]
                //Expander.create [
                //    Expander.dock Dock.Top
                //    Expander.header "Logs de conversion"
                //    Expander.content (
                //        TextBox.create [ 
                //            TextBox.textWrapping TextWrapping.Wrap
                //            TextBox.isReadOnly true
                //            TextBox.text (
                //                convertionReadyness
                //                |> function 
                //                    | Error e -> sprintf "Erreurs à résoudre avant le lancer : \n%A" e
                //                    | Ok _ ->  
                //                        match state.Conversion with
                //                        | NotStarted -> sprintf "Conversion pas démarrée"
                //                        | Started -> sprintf "Conversion en cours..."
                //                        | Resolved r -> 
                //                            match r with
                //                            | Ok c -> sprintf "\
                //                                        La conversion a réussi\n\
                //                                        Durée : %s" c.Duration
                //                            | Error e -> sprintf "\
                //                                            La conversion a échoué. Erreurs : \n\
                //                                            %A" e
                //            )
                //        ]
                //    )   
                //]
                
            ]
        ]
        


