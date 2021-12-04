namespace DronePublish.Core.Test

open DronePublish.Core
open Expecto
open Expecto.Flip

module EditProfileTests =
    [<Tests>]
    let newProfileTests =
        testList "Nouveau profile" [
            testCase "Cas Cancel" <| fun _ ->
                let initialState = TestHelpers.initTestState "" "" "" "" List.empty
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                
                let (resultState, resultCmd) = updateWithServices (ProfileEdited (0, Empty)) initialState

                resultState |> Expect.equal "Etat initial" initialState
                resultCmd |> Expect.isEmpty "Pas de commande"

            testCase "Cas passant : pas de profile préexistant" <| fun _ ->
                let initialState = TestHelpers.initTestState "" "" "" ""  List.empty
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                let newProfile = NotSelected {
                    Nom = NonEmptyString100 "Nouveau profile"
                    Suffixe = NonEmptyString100 "_NewProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                
                let (resultState, resultCmd) = updateWithServices (ProfileEdited (0, newProfile)) initialState

                resultState |> Expect.equal "Profile ajouté à la liste" { initialState with Profiles = [ newProfile ] }
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Sauvegarde" [| SaveState |]

            testCase "Cas passant : avec un profile préexistant" <| fun _ ->
                let existingProfile = NotSelected {
                    Nom = NonEmptyString100 "Profile existant"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = TestHelpers.initTestState "" "" "" "" [ existingProfile ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                let newProfile = NotSelected {
                    Nom = NonEmptyString100 "Nouveau profile"
                    Suffixe = NonEmptyString100 "_NewProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                
                let (resultState, resultCmd) = updateWithServices (ProfileEdited (0, newProfile)) initialState

                resultState |> Expect.equal "Profile ajouté à la liste" { initialState with Profiles = initialState.Profiles @ [ newProfile ] }
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Sauvegarde" [| SaveState |]
                
        ]

