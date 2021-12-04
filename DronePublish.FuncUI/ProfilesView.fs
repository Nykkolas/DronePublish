namespace DronePublish.FuncUI

open DronePublish.Core
open Avalonia.FuncUI.DSL
open Avalonia.Controls

module ProfilesView =
    
    let view state dispatch =
        DockPanel.create [
            DockPanel.row 1
            DockPanel.column 0
            DockPanel.margin 10.0
            DockPanel.children [
                Button.create [
                    Button.dock Dock.Bottom
                    Button.content "Nouveau"
                    Button.width 60.0
                    Button.onClick (fun _ -> EditProfile (0, Empty) |> dispatch)
                ]
                match state.Profiles with
                | [] -> 
                    TextBlock.create [
                        TextBlock.dock Dock.Top
                        TextBlock.text "Pas de profile"
                    ]
                | _ -> 
                    TextBlock.create [
                        TextBlock.dock Dock.Top
                        TextBlock.text (sprintf "Un profile :\n%A" state.Profiles)
                    ] 
            ]
        ]

