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
                
                let (resultState, resultCmd) = updateWithServices (ProfileEdited (-1, newProfile)) initialState

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
                
                let (resultState, resultCmd) = updateWithServices (ProfileEdited (-1, newProfile)) initialState

                resultState |> Expect.equal "Profile ajouté à la liste" { initialState with Profiles = initialState.Profiles @ [ newProfile ] }
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Sauvegarde" [| SaveState |]       
        ]

    [<Tests>]
    let deleteProfile =
        testList "Suppression d'un profile" [
            testCase "Cas Passant : il reste un profile après" <| fun _ ->
                let existingProfile1 = NotSelected {
                    Nom = NonEmptyString100 "Profile existant 1"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let existingProfile2 = NotSelected {
                    Nom = NonEmptyString100 "Profile existant 2"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = TestHelpers.initTestState "" "" "" "" [ existingProfile1; existingProfile2 ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs

                let (resultState, resultCmd) = updateWithServices (DeleteProfile 1) initialState

                resultState |> Expect.equal "L'état est le même sans le second profile" { initialState with Profiles = [ existingProfile1 ] }
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Sauvegarde" [| SaveState |]

            testCase "Cas Passant : plus de profile après" <| fun _ ->
                let existingProfile1 = NotSelected {
                    Nom = NonEmptyString100 "Profile existant 1"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = TestHelpers.initTestState "" "" "" "" [ existingProfile1 ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs

                let (resultState, resultCmd) = updateWithServices (DeleteProfile 0) initialState

                resultState |> Expect.equal "L'état est le même sans le second profile" { initialState with Profiles = [] }
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Sauvegarde" [| SaveState |]
        ]

    [<Tests>]
    let editProfileTests =
        testList "Modification d'un profile" [
            testCase "Cas passant : modification d'un profile" <| fun _ ->
                let existingProfile = NotSelected {
                    Nom = NonEmptyString100 "Profile existant"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let modifiedProfile = NotSelected {
                    Nom = NonEmptyString100 "Profile modifié"
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

                let (resultState, resultCmd) = updateWithServices (ProfileEdited (0, modifiedProfile)) initialState

                resultState |> Expect.equal "Le profile a été mis à jour" { initialState with Profiles = [ modifiedProfile ] }
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Sauvegarde" [| SaveState |] 
        ]
