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
                            if (sprintf @"%s\ffmpeg.exe" state.Conf.ExecutablesPath |> File.Exists) then
                                TextBlock.foreground "Green"
                                TextBlock.text "Executables OK"
                            else 
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
                DockPanel.create [
                    DockPanel.row 1
                    DockPanel.column 1
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
                            TextBox.text state.DestDir
                        ]
                    ]
                ]

            ]
        ]
        