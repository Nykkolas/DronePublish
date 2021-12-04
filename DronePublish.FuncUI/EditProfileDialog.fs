namespace DronePublish.FuncUI

open DronePublish.Core
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Components.Hosts
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open Elmish
open Avalonia.FuncUI.Elmish
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator

module EditProfileDialog =
    type NotValidatedProfileData = {
        Nom: string * Validation<NonEmptyString100,StringError>
        Suffixe: string * Validation<NonEmptyString100,StringError>
        Bitrate: string * Result<PositiveLong,IntError>
        Width: string * Result<PositiveInt,IntError>
        Height: string * Result<PositiveInt,IntError>
        Codec: Codec
    }

    type Model = {
        Dialog: HostWindow
        Index: int
        Profile: Profile
        NotValidatedProfileData: NotValidatedProfileData
    }

    type Msg =
        | Cancel
        | Enregistrer
        | UpdateNom of string
        | UpdateSuffixe of string
        | UpdateBitrate of string
        | UpdateWidth of string
        | UpdateHeight of string

    let init ((dialog:HostWindow), (indexProfile:int * Profile)) =
        let create (profileData:ProfileData) =
            {
                Nom = (NonEmptyString100.unWrap profileData.Nom, profileData.Nom |> Ok)
                Suffixe = (NonEmptyString100.unWrap profileData.Suffixe, profileData.Suffixe |> Ok)
                Bitrate = (PositiveLong.toString profileData.Bitrate, profileData.Bitrate |> Ok)
                Width = (PositiveInt.toString profileData.Width, profileData.Width |> Ok)
                Height = (PositiveInt.toString profileData.Height, profileData.Height |> Ok)
                Codec = profileData.Codec
            }
        
        let (index, profile) = indexProfile
        let notValidatedData = 
            match Profile.getProfileData profile with
            | None -> 
                {
                    Nom = ("", Error [ IsNullOrEmpty ])
                    Suffixe = ("", Error [ IsNullOrEmpty ])
                    Bitrate = ("", Error IsLowerOrEqualThanZero)
                    Width = ("", Error IsLowerOrEqualThanZero)
                    Height = ("", Error IsLowerOrEqualThanZero)
                    Codec = H264
                }
            | Some p -> create p

        { Dialog = dialog; Index = index; Profile = profile; NotValidatedProfileData = notValidatedData }

    let update msg state =
        match msg with
        | Cancel ->
            state.Dialog.Close ((0, Empty))
            state

        | Enregistrer -> 
            let unWrapNotValidatedString = function
            | _, Ok s -> s
            | _, Error _ -> NonEmptyString100 "#Erreur impossible#"
            
            let unWrapNotValidatedInt = function
            | _, Ok i -> i
            | _, Error _ -> PositiveInt 0 // Cas théroriquement impossible

            let bitrate =
                match state.NotValidatedProfileData.Bitrate with
                | _, Ok i -> i
                | _, Error _ -> PositiveLong 0L // Cas théroriquement impossible

            let newProfile = 
                match state.Profile with
                | Empty -> 
                    NotSelected (
                        Profile.createData 
                            (unWrapNotValidatedString state.NotValidatedProfileData.Nom)
                            (unWrapNotValidatedString state.NotValidatedProfileData.Suffixe)
                            bitrate 
                            (unWrapNotValidatedInt state.NotValidatedProfileData.Width)
                            (unWrapNotValidatedInt state.NotValidatedProfileData.Height)
                            state.NotValidatedProfileData.Codec
                    )
                | p -> Empty

            state.Dialog.Close((0, newProfile))
            state

        | UpdateNom text ->
            let validatedNom = NonEmptyString100.tryCreate text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Nom = (text, validatedNom) } }

        | UpdateSuffixe text ->
            let validatedSuffixe = NonEmptyString100.tryCreate text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Suffixe = (text, validatedSuffixe) } }

        | UpdateBitrate text ->
            let validatedBitrate = PositiveLong.tryParse text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Bitrate = (text, validatedBitrate) } }

        | UpdateWidth text ->
            let validatedWidth = PositiveInt.tryParse text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Width = (text, validatedWidth) } }

        | UpdateHeight text ->
            let validatedHeight = PositiveInt.tryParse text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Height = (text, validatedHeight) } }

    let isValid notValidatedProfileData =
        notValidatedProfileData.Nom 
        |> (fun (_, nom) -> nom |> Result.isOk)

    let ligne state dispatch label msgFunc field =
        DockPanel.create [
            DockPanel.margin 10.0
            DockPanel.children [
                TextBlock.create [ 
                    TextBlock.dock Dock.Left
                    TextBlock.width 50.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.margin (5.0, 0.0)
                    TextBlock.dock Dock.Left
                    TextBlock.text label 
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Right
                    TextBlock.width 5.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.margin (5.0, 0.0)
                    TextBlock.dock Dock.Right
                    match field with
                    | _, Ok _ -> TextBlock.text ""
                    | _, Error e -> 
                        TextBlock.text "!"
                        TextBlock.tip (sprintf "%A" e)
                ]
                TextBox.create [
                    TextBlock.dock Dock.Right
                    TextBox.verticalAlignment VerticalAlignment.Center
                    TextBox.margin (5.0, 0.0)
                    TextBox.dock Dock.Right
                    TextBox.onTextChanged (fun t -> msgFunc t |> dispatch)
                ]
            ]
        ]

    let view state dispatch =
        Border.create [
            Border.borderThickness 2.0
            Border.borderBrush "red"
            Border.child (
                StackPanel.create [
                    StackPanel.margin 10.0
                    StackPanel.children [
                        TextBlock.create [
                            TextBlock.textAlignment TextAlignment.Center
                            TextBlock.text (
                                match state.Profile with
                                | Empty -> "Création d'un nouveau profile"
                                | Selected p | NotSelected p -> sprintf "Editer le profile %s" (NonEmptyString100.unWrap p.Nom)
                            )
                        ]

                        ligne state dispatch "Nom : " UpdateNom state.NotValidatedProfileData.Nom
                                    
                        ligne state dispatch "Suffixe :" UpdateSuffixe state.NotValidatedProfileData.Suffixe
                    
                        ligne state dispatch "Bitrate :" UpdateBitrate state.NotValidatedProfileData.Bitrate
                    
                        ligne state dispatch "Largeur :" UpdateWidth state.NotValidatedProfileData.Width
                            
                        ligne state dispatch "Hauteur :" UpdateHeight state.NotValidatedProfileData.Height

                        StackPanel.create [
                            StackPanel.orientation Orientation.Horizontal
                            StackPanel.horizontalAlignment HorizontalAlignment.Right
                            StackPanel.children [
                                Button.create [
                                    Button.margin 10.0
                                    Button.content "Annuler"
                                    Button.onClick (fun _ -> dispatch Cancel)
                                ]
                                Button.create [
                                    Button.margin 10.0
                                    Button.content "Ok"
                                    isValid state.NotValidatedProfileData |> Button.isEnabled 
                                    Button.onClick (fun _ -> dispatch Enregistrer)
                                ]
                            ]
                        ]
                    ]

                ]
            )
        ]
        
    type EditProfileDialog (indexProfile:(int * Profile)) as this = 
        inherit HostWindow ()

        do
            this.CanResize <- false
            this.HasSystemDecorations <- false
            this.SizeToContent <- SizeToContent.Height
            this.Width <- 350.0
            this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
           
            Program.mkSimple init update view
            |> Program.withHost this
            |> Program.runWith (this, indexProfile)

    let show (window:Window) (indexProfile:int * Profile) =
        Dispatcher.UIThread.InvokeAsync<int * Profile>(fun _ ->
            let infoDialog = EditProfileDialog (indexProfile)
            infoDialog.ShowDialog<int * Profile> window)
        |> Async.AwaitTask
