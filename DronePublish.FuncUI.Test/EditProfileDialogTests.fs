namespace DronePublish.FuncUI.Test

open DronePublish.FuncUI
open Expecto
open Expecto.Flip
open DronePublish.Core

module EditProfileDialogTests =
    [<Tests>]
    let isValidTests =
        testList "isValid" [
            testCase "Tout est bon" <| fun _ ->
                let (notValidatedProfileData:EditProfileDialog.NotValidatedProfileData) = {
                    Nom = ("nom", Ok (NonEmptyString100 "nom"))
                    Suffixe = ("suffixe", Ok (NonEmptyString100 "suffixe"))
                    Bitrate = ("10", Ok (PositiveLong 10L))
                    Width =  ("1920", Ok (PositiveInt 1920))
                    Height = ("1080", Ok (PositiveInt 1080))
                    Codec = H264
                }

                EditProfileDialog.isValid notValidatedProfileData |> Expect.isTrue "Profile valide"

            testCase "Une erreur" <| fun _ ->
                let (notValidatedProfileData:EditProfileDialog.NotValidatedProfileData) = {
                    Nom = ("nom", Ok (NonEmptyString100 "nom"))
                    Suffixe = ("suffixe", Ok (NonEmptyString100 "suffixe"))
                    Bitrate = ("10", Ok (PositiveLong 10L))
                    Width =  ("1920", Ok (PositiveInt 1920))
                    Height = ("", Error ErrorParsingText )
                    Codec = H264
                }

                EditProfileDialog.isValid notValidatedProfileData |> Expect.isFalse "Profile invalide"
        ]

