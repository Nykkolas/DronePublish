namespace DronePublish.FuncUI

open DronePublish.Core
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.FuncUI.Components
open Elmish

(* TODO : Sélectionner tout / aucun *)

module ProfilesView =
    let profileTemplate (index, profile) dispatch =
        match profile with 
        | Empty -> 
            DockPanel.create [
                DockPanel.children [ TextBlock.create [ TextBlock.text "Pas de profile" ] ]
            ]
        | Selected p | NotSelected p ->
            DockPanel.create [
                DockPanel.margin (5.0, 5.0)
                DockPanel.tip (sprintf "\
                                    Suffixe : %s\n\
                                    Bitrate : %i\n\
                                    Resolution : %ix%i\n\
                                    Codec : %A" 
                                    (NonEmptyString100.unWrap p.Suffixe) 
                                    (PositiveLong.unWrap p.Bitrate) 
                                    (PositiveInt.unWrap p.Width)
                                    (PositiveInt.unWrap p.Height)
                                    p.Codec
                                )
                DockPanel.children [
                    
                    CheckBox.create [
                        CheckBox.dock Dock.Left
                        CheckBox.margin (5.0, 0.0)
                        Profile.isSelected profile |> CheckBox.isChecked
                        CheckBox.onChecked (fun _ -> ProfilesCore.Msg.CheckProfile index |> dispatch)
                        CheckBox.onUnchecked (fun _ -> ProfilesCore.Msg.UnCheckProfile index |> dispatch)
                    ]

                    Button.create [
                        Button.dock Dock.Right
                        Button.margin (5.0, 0.0)
                        Button.width 25.0
                        Button.content "..."
                        Button.onClick (fun _ -> ProfilesCore.Msg.EditProfile index |> dispatch)
                    ]

                    Button.create [
                        Button.dock Dock.Right
                        Button.margin (5.0, 0.0)
                        Button.width 25.0
                        Button.content "-"
                        Button.onClick (fun _ -> dispatch (ProfilesCore.Msg.DeleteProfile index))
                    ]

                    TextBlock.create [
                        TextBlock.dock Dock.Left
                        TextBlock.verticalAlignment VerticalAlignment.Center
                        TextBlock.margin (5.0, 0.0)
                        TextBlock.text (NonEmptyString100.unWrap p.Nom)
                    ]
                ]
            ]

    let view state dispatch =
        DockPanel.create [
            DockPanel.row 1
            DockPanel.column 0
            DockPanel.margin 10.0
            DockPanel.children [
                StackPanel.create [
                    StackPanel.dock Dock.Bottom
                    StackPanel.orientation Orientation.Horizontal
                    StackPanel.horizontalAlignment HorizontalAlignment.Right
                    StackPanel.children [
                        Button.create [
                            
                            Button.margin 10.0
                            Button.content "Nouveau"
                            Button.width 60.0
                            Button.onClick (fun _ -> ProfilesCore.Msg.EditProfile -1 |> dispatch)
                        ]

                        Button.create [
                            Button.dock Dock.Bottom
                            Button.margin 10.0
                            Button.content "Tout désélectionner"
                            Button.width 80.0
                            Button.onClick (fun _ -> ProfilesCore.Msg.UnCheckAllProfiles |> dispatch)
                        ]

                        Button.create [
                            Button.dock Dock.Bottom
                            Button.margin 10.0
                            Button.content "Tout sélectionner"
                            Button.width 80.0
                            Button.onClick (fun _ -> ProfilesCore.Msg.CheckAllProfiles |> dispatch)
                        ]
                    ]
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
