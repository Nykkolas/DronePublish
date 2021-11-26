namespace DronePublish.FuncUI

open DronePublish.Core
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open System.IO

module View = 
    let view (state:Model) dispatch =
        Grid.create [
            Grid.row 2
            Grid.column 2
            Grid.rowDefinitions "50,*"
            Grid.columnDefinitions "*,2*"
            //Grid.showGridLines true
            Grid.children [
                (* Exécutables *)
                StackPanel.create [
                    StackPanel.row 0
                    StackPanel.column 0
                    StackPanel.orientation Orientation.Horizontal
                    StackPanel.margin 10.0
                    StackPanel.children [
                        TextBlock.create [
                            TextBlock.verticalAlignment VerticalAlignment.Center
                            TextBlock.fontWeight FontWeight.Bold
                            TextBlock.margin (0.0, 0.0, 5.0, 0.0)
                            match Model.validateExecutablePath state.Conf.ExecutablesPath with
                            | Ok _ ->
                                TextBlock.foreground "Green"
                                TextBlock.text "Executables OK"
                            | Error _->
                                TextBlock.foreground "Red"
                                TextBlock.text "Executables KO"
                        ]
                        Button.create [
                            Button.verticalAlignment VerticalAlignment.Center
                            Button.width 20.0
                            Button.content "..."
                            Button.onClick (fun _ -> ChooseExecutablesPath |> dispatch)
                        ]
                    ]
                ]
                StackPanel.create [
                    StackPanel.row 1
                    StackPanel.column 1
                    StackPanel.children [
                        TextBlock.create [
                            //TextBlock.fontFamily (FontFamily.Parse "Arial")
                            TextBlock.text "Fichier à convertir :"
                        ]
                        DockPanel.create [
                            DockPanel.margin 10.0
                            DockPanel.children [
                                Expander.create [
                                    Expander.dock Dock.Bottom
                                    Expander.header "Informations"
                                    Expander.content (
                                        let text =
                                            match state.SourceInfos with
                                            | NotStarted -> "Pas d'info"
                                            | Started -> "En cours..."
                                            | Resolved r -> 
                                                match r with 
                                                | Ok s -> sprintf "Done\nCodec : %s" s.Codec
                                                | Error e -> sprintf "Erreur : %A" e
                                        TextBlock.create [ TextBlock.text text ]
                                    )
                                ]
                                Button.create [
                                    Button.verticalAlignment VerticalAlignment.Center
                                    Button.dock Dock.Right
                                    Button.width 20.0
                                    Button.content "..."
                                    Button.onClick (fun _ -> ChooseSourceFile |> dispatch)
                                    Button.isEnabled (
                                        match state.SourceInfos with
                                        | NotStarted | Resolved _ -> true
                                        | Started -> false
                                    )
                                ]
                                TextBox.create [
                                    TextBox.verticalAlignment VerticalAlignment.Center
                                    TextBox.dock Dock.Left
                                    TextBlock.isEnabled false
                                    TextBox.text state.SourceFile
                                ]
                            ]
                        ]
                        TextBlock.create [
                            TextBlock.text "Répertoire de destination :"
                        ]
                        DockPanel.create [
                            DockPanel.margin 10.0
                            DockPanel.children [
                                Button.create [
                                    Button.verticalAlignment VerticalAlignment.Center
                                    Button.dock Dock.Right
                                    Button.width 20.0
                                    Button.content "..."
                                    Button.onClick (fun _ -> ChooseDestDir |> dispatch)
                                ]
                                TextBox.create [
                                    TextBox.verticalAlignment VerticalAlignment.Center
                                    TextBox.dock Dock.Left
                                    TextBlock.isEnabled false
                                    TextBox.text state.DestDir
                                ]
                            ]
                        ]
                    ]
                ]

            ]
        ]
        