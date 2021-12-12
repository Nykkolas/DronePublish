namespace DronePublish.Core.Test

open Expecto
open Expecto.Flip
open DronePublish.Core

module ProfilesCoreTests =
    [<Tests>]
    let newProfileTests =
        testList "Nouveau profile" [
            testCase "Cas Cancel" <| fun _ ->
                let initialState = List.empty
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    ProfilesCore.update message state dialogs
                
                let (resultState, resultCmd, resultExternal) = updateWithServices (ProfilesCore.Msg.ProfileEdited (0, Empty)) initialState

                resultState |> Expect.equal "Etat initial" initialState
                resultCmd |> Expect.isEmpty "Pas de commande"
                resultExternal |> Expect.isNone "Pas de commande au parent"

            testCase "Cas passant : pas de profile préexistant" <| fun _ ->
                let initialState = List.empty
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    ProfilesCore.update message state dialogs
                let newProfile = NotSelected {
                    Nom = NonEmptyString100 "Nouveau profile"
                    Suffixe = NonEmptyString100 "_NewProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                
                let (resultState, resultCmd, resultExternal) = updateWithServices (ProfilesCore.Msg.ProfileEdited (-1, newProfile)) initialState

                resultState |> Expect.equal "Profile ajouté à la liste" [ newProfile ]
                resultCmd |> Expect.isEmpty "Pas de nouvelle commande"
                resultExternal |> Expect.wantSome "Une action est prévue" |> Expect.equal "C'est une sauvegarde" ProfilesCore.ExternalMsg.SaveState

            testCase "Cas passant : avec un profile préexistant" <| fun _ ->
                let existingProfile = NotSelected {
                    Nom = NonEmptyString100 "Profile existant"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = [ existingProfile ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    ProfilesCore.update message state dialogs
                let newProfile = NotSelected {
                    Nom = NonEmptyString100 "Nouveau profile"
                    Suffixe = NonEmptyString100 "_NewProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                
                let (resultState, resultCmd, resultExternal) = updateWithServices (ProfilesCore.Msg.ProfileEdited (-1, newProfile)) initialState

                resultState |> Expect.equal "Profile ajouté à la liste" (initialState @ [ newProfile ])
                resultExternal |> Expect.wantSome "Il y a une commande externe" |> Expect.equal "Cette commande est sauvegarde" ProfilesCore.ExternalMsg.SaveState
                resultCmd |> Expect.isEmpty "Pas d'autre commande"
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
                let initialState = [ existingProfile ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    ProfilesCore.update message state dialogs

                let (resultState, resultCmd, resultExternal) = updateWithServices (ProfilesCore.Msg.ProfileEdited (0, modifiedProfile)) initialState

                resultState |> Expect.equal "Le profile a été mis à jour" [ modifiedProfile ]
                resultExternal |> Expect.wantSome "Il y a une commande externe" |> Expect.equal "Cette commande est sauvegarde" ProfilesCore.ExternalMsg.SaveState
                resultCmd |> Expect.isEmpty "Pas d'autre commande"
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
                let initialState = [ existingProfile1; existingProfile2 ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    ProfilesCore.update message state dialogs

                let (resultState, resultCmd, resultExternal) = updateWithServices (ProfilesCore.Msg.DeleteProfile 1) initialState

                resultState |> Expect.equal "L'état est le même sans le second profile" [ existingProfile1 ]
                resultExternal |> Expect.wantSome "Il y a une commande externe" |> Expect.equal "Cette commande est sauvegarde" ProfilesCore.ExternalMsg.SaveState
                resultCmd |> Expect.isEmpty "Pas d'autre commande"

            testCase "Cas Passant : plus de profile après" <| fun _ ->
                let existingProfile1 = NotSelected {
                    Nom = NonEmptyString100 "Profile existant 1"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = [ existingProfile1 ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    ProfilesCore.update message state dialogs

                let (resultState, resultCmd, resultExternal) = updateWithServices (ProfilesCore.Msg.DeleteProfile 0) initialState

                resultState |> Expect.isEmpty "Liste vide"
                resultExternal |> Expect.wantSome "Il y a une commande externe" |> Expect.equal "Cette commande est sauvegarde" ProfilesCore.ExternalMsg.SaveState
                resultCmd |> Expect.isEmpty "Pas d'autre commande"
        ]

    [<Tests>]
    let checkTests =
        testList "Sélection / Désélection" [
            testCase "Sélection d'un profile" <| fun _ ->
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
                let initialState = [ existingProfile1; existingProfile2 ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    ProfilesCore.update message state dialogs

                let (resultState, resultCmd, resultExternal) = updateWithServices (ProfilesCore.Msg.CheckProfile 1) initialState
                
                resultState |> Expect.equal "Le profile a été coché" [ existingProfile1; Profile.select existingProfile2 ]
                resultExternal |> Expect.wantSome "Il y a une commande externe" |> Expect.equal "Cette commande est sauvegarde" ProfilesCore.ExternalMsg.SaveState
                resultCmd |> Expect.isEmpty "Pas d'autre commande"

            testCase "Désélection d'un profile" <| fun _ ->
                let existingProfile1 = NotSelected {
                    Nom = NonEmptyString100 "Profile existant 1"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let existingProfile2 = Selected {
                    Nom = NonEmptyString100 "Profile existant 2"
                    Suffixe = NonEmptyString100 "_ExistsProfile"
                    Bitrate = PositiveLong 10000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = [ existingProfile1; existingProfile2 ]
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    ProfilesCore.update message state dialogs

                let (resultState, resultCmd, resultExternal) = updateWithServices (ProfilesCore.Msg.UnCheckProfile 1) initialState
                
                resultState |> Expect.equal "Le profile a été coché" [ existingProfile1; Profile.unSelect existingProfile2 ]
                resultExternal |> Expect.wantSome "Il y a une commande externe" |> Expect.equal "Cette commande est sauvegarde" ProfilesCore.ExternalMsg.SaveState
                resultCmd |> Expect.isEmpty "Pas d'autre commande"
        ]