namespace DronePublish.FuncUI

open DronePublish.Core
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media

module View = 
    let executables state dispatch =
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
                    match Conversion.validateExecutablePath state.Conf.ExecutablesPath with
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

    let sourceFile state dispatch =
        DockPanel.create [
            DockPanel.dock Dock.Top
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
                                | Ok s -> sprintf "\
                                    Resolution : %ix%i\n\
                                    Codec : %s\n\
                                    Bitrate : %i" s.Width s.Height s.Codec (int s.Bitrate)
                                | Error e -> sprintf "Il y a des erreurs :\n %A" e
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

    let destDir (state:Model) dispatch =
        DockPanel.create [
            DockPanel.dock Dock.Top
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

    let view (state:Model) dispatch =
        Grid.create [
            Grid.row 2
            Grid.column 2
            Grid.rowDefinitions "50,*"
            Grid.columnDefinitions "2*,3*"
            Grid.isEnabled (not (ConversionJobs.isStarted state.ConversionJobs))
            //Grid.showGridLines true
            Grid.children [
                (* Exécutables *)
                executables state dispatch

                //Button.create [
                //    Button.row 0
                //    Button.column 1
                //    Button.content "Dialog"
                //    Button.onClick (fun _ -> dispatch ShowDialog)
                //]

                ProfilesView.view state (ProfilesMsg >> dispatch)

                DockPanel.create [
                    DockPanel.row 1
                    DockPanel.column 1
                    DockPanel.children [
                        (* Fichiers à convertir *)
                        TextBlock.create [
                            TextBlock.dock Dock.Top
                            TextBlock.text "Fichier à convertir :"
                        ]
                        sourceFile state dispatch

                        (* Répertoire de destination *)
                        TextBlock.create [
                            TextBlock.dock Dock.Top
                            TextBlock.text "Répertoire de destination :"
                        ]
                        destDir state dispatch

                        (* Bouton de convertion *)
                        ConversionView.view state dispatch
                    ]
                ]
            ]
        ]
        