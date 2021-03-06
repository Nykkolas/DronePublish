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

(* TODO : boite de dialogue plus jolie : bordure ? couleur ? *)
(* TODO : comment présenter la résolution ? *)

module EditProfileDialog =
    type NotValidatedProfileData = {
        Nom: string * Result<NonEmptyString100,StringError list>
        Suffixe: string * Result<NonEmptyString100,StringError list>
        Bitrate: string * Result<PositiveLong,ProfileError>
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
        | UpdateCodec of Codec

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
            | Some p -> create p
            | None -> 
                {
                    Nom = ("", Error [ IsNullOrEmpty ])
                    Suffixe = ("", Error [ IsNullOrEmpty ])
                    Bitrate = ("", Error (PositiveLongBitrateError ErrorParsingText))
                    Width = ("", Error ErrorParsingText)
                    Height = ("", Error ErrorParsingText)
                    Codec = H264
                }

        { Dialog = dialog; Index = index; Profile = profile; NotValidatedProfileData = notValidatedData }

    let update msg state =
        match msg with
        | Cancel ->
            state.Dialog.Close ((0, Empty))
            state

        (* TODO : mais si les cas impossibles arrivent quand même ? (genre j'oublie de modifier la fonction isValid) *)
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

            let extractProfileData notValidatedProfileData =
                Profile.createData 
                    (unWrapNotValidatedString notValidatedProfileData.Nom)
                    (unWrapNotValidatedString notValidatedProfileData.Suffixe)
                    bitrate 
                    (unWrapNotValidatedInt notValidatedProfileData.Width)
                    (unWrapNotValidatedInt notValidatedProfileData.Height)
                    notValidatedProfileData.Codec

            let newProfile = 
                match state.Profile with
                | Empty -> NotSelected (extractProfileData state.NotValidatedProfileData)
                | Selected _ -> Selected (extractProfileData state.NotValidatedProfileData)
                | NotSelected _ -> NotSelected (extractProfileData state.NotValidatedProfileData)

            state.Dialog.Close((state.Index, newProfile))
            state

        | UpdateNom text ->
            let validatedNom = NonEmptyString100.tryCreate text
            { state with 
                NotValidatedProfileData = { state.NotValidatedProfileData with Nom = (text, validatedNom) } 
            }

        | UpdateSuffixe text ->
            let validatedSuffixe = NonEmptyString100.tryCreate text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Suffixe = (text, validatedSuffixe) } }

        | UpdateBitrate text ->
            let validatedBitrate = Profile.tryParseBitrate text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Bitrate = (text, validatedBitrate) } }

        | UpdateWidth text ->
            let validatedWidth = PositiveInt.tryParse text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Width = (text, validatedWidth) } }

        | UpdateHeight text ->
            let validatedHeight = PositiveInt.tryParse text
            { state with NotValidatedProfileData = { state.NotValidatedProfileData with Height = (text, validatedHeight) } }

        | UpdateCodec codec ->
            let notValidatedProfileData = { state.NotValidatedProfileData with Codec = codec }
            { state with NotValidatedProfileData = notValidatedProfileData }

    let isValid notValidatedProfileData =
        let fieldToBool (_, result) =
            result |> Result.isOk

        [ 
            fieldToBool notValidatedProfileData.Nom
            fieldToBool notValidatedProfileData.Suffixe
            fieldToBool notValidatedProfileData.Bitrate
            fieldToBool notValidatedProfileData.Width
            fieldToBool notValidatedProfileData.Height
        ]
        |> List.forall id 
        (* TODO : Ajouter un test pour vérifier que le bon nombre de champs sont testés (possible ?) *)

    let ligne state dispatch label msgFunc (text, result) =
        DockPanel.create [
            DockPanel.margin 10.0
            DockPanel.children [
                TextBlock.create [ 
                    TextBlock.dock Dock.Left
                    TextBlock.width 70.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.margin (5.0, 0.0)
                    TextBlock.text label 
                ]
                TextBlock.create [
                    TextBlock.dock Dock.Right
                    TextBlock.width 5.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.margin (5.0, 0.0)
                    match result with
                    | Ok _ -> TextBlock.text ""
                    | Error e -> 
                        TextBlock.text "!"
                        TextBlock.tip (sprintf "%A" e)
                ]
                TextBox.create [
                    TextBox.verticalAlignment VerticalAlignment.Center
                    TextBox.margin (5.0, 0.0)
                    TextBox.dock Dock.Right
                    TextBox.text text
                    TextBox.onTextChanged (fun t -> msgFunc t |> dispatch)
                ]
            ]
        ]

    let codec state dispatch =
        DockPanel.create [
            DockPanel.margin 10.0
            DockPanel.children [
                TextBlock.create [ 
                    TextBlock.dock Dock.Left
                    TextBlock.width 70.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.margin (5.0, 0.0)
                    TextBlock.text "Codec" 
                ]

                TextBlock.create [
                    TextBlock.dock Dock.Right
                    TextBlock.width 5.0
                    TextBlock.verticalAlignment VerticalAlignment.Center
                    TextBlock.margin (5.0, 0.0)
                    TextBlock.text ""
                ]

                ComboBox.create [
                    ComboBox.dock Dock.Right
                    ComboBox.margin (5.0, 0.0)
                    ComboBox.dataItems [
                        H264
                        H264_NVENC
                    ]
                    ComboBox.selectedItem state.NotValidatedProfileData.Codec
                    ComboBox.onSelectedItemChanged (
                        tryUnbox >> Option.iter (UpdateCodec >> dispatch)
                    )
                ]
            ]
        ]

    let view state dispatch =
        Border.create [
            //Border.borderThickness 2.0
            //Border.borderBrush "red"
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
                    
                        ligne state dispatch "Bitrate (bps) :" UpdateBitrate state.NotValidatedProfileData.Bitrate
                    
                        ligne state dispatch "Largeur :" UpdateWidth state.NotValidatedProfileData.Width
                            
                        ligne state dispatch "Hauteur :" UpdateHeight state.NotValidatedProfileData.Height

                        codec state dispatch

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
                                    Button.content "Enregistrer"
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
            //this.HasSystemDecorations <- false
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
