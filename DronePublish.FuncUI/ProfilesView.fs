namespace DronePublish.FuncUI

open DronePublish.Core
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.FuncUI.Components

module ProfilesView =
    let profileTemplate profile dispatch =
        DockPanel.create [
            DockPanel.children [
                TextBlock.create [
                    TextBlock.dock Dock.Left
                    match profile with 
                    | Empty -> TextBlock.text "Pas de profile"
                    | Selected p | NotSelected p -> 
                        TextBlock.text (NonEmptyString100.unWrap p.Nom)
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
                    Button.onClick (fun _ -> EditProfile (0, Empty) |> dispatch)
                ]
                match state.Profiles with
                | [] -> 
                    TextBlock.create [

                        TextBlock.dock Dock.Top
                        TextBlock.text "Pas de profile"
                    ]
                | _ -> 
                    ListBox.create [
                        ListBox.dataItems state.Profiles
                        ListBox.itemTemplate (DataTemplateView<Profile>.create (fun item -> profileTemplate item dispatch))
                    ]
            ]
        ]
