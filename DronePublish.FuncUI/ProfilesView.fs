namespace DronePublish.FuncUI

open DronePublish.Core
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.FuncUI.Components

module ProfilesView =
    let profileTemplate (index, profile) dispatch =
        match profile with 
        | Empty -> 
            DockPanel.create [
                DockPanel.children [ TextBlock.create [ TextBlock.text "Pas de profile" ] ]
            ]
        | Selected p | NotSelected p ->
            DockPanel.create [
                DockPanel.children [
                    Button.create [
                        Button.dock Dock.Right
                        Button.margin (5.0, 0.0)
                        Button.content "..."
                        Button.onClick (fun _ -> EditProfile index |> dispatch)
                    ]

                    Button.create [
                        Button.dock Dock.Right
                        Button.margin (5.0, 0.0)
                        Button.content "-"
                        Button.onClick (fun _ -> dispatch (DeleteProfile index))
                    ]

                    TextBlock.create [
                        TextBlock.dock Dock.Left
                        TextBlock.text (sprintf "%i : %s" index (NonEmptyString100.unWrap p.Nom))
                        TextBlock.tip (sprintf "\
                                            Suffixe : %s\n\
                                            Bitrate : %i\n\
                                            Resolution : %ix%i" 
                                            (NonEmptyString100.unWrap p.Suffixe) 
                                            (PositiveLong.unWrap p.Bitrate) 
                                            (PositiveInt.unWrap p.Width)
                                            (PositiveInt.unWrap p.Height)
                                        )
                    ]
                ]
            ]

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
                    Button.onClick (fun _ -> EditProfile -1 |> dispatch)
                ]
                match state.Profiles with
                | [] -> 
                    TextBlock.create [

                        TextBlock.dock Dock.Top
                        TextBlock.text "Pas de profile"
                    ]
                | _ -> 
                    ListBox.create [
                        List.indexed state.Profiles |> ListBox.dataItems
                        ListBox.itemTemplate (DataTemplateView<int * Profile>.create (fun item -> profileTemplate item dispatch))
                    ]
            ]
        ]
