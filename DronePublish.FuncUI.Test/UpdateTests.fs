namespace DronePublish.FuncUI.Test

open Expecto
open Expecto.Flip
open DronePublish.FuncUI
open DronePublish.Core.Test
open Elmish

module UpdateTests =
    [<Tests>]
    let tests =
        testList "Répertoire d'exécutables" [
            testCase "Cas passant : état mis à jour" <| fun _ ->
                let saveFile = ModelTests.generateSaveFileName ()
                let initialState = ModelTests.initTestState saveFile "" "" ""
                let exePath = @"C:\Exe\Path"
                let dialogs = DialogsTest.create exePath ""
                let updateWithServices message state =
                    Update.update message state dialogs
                let message = Msg.ExecutablesPathChosen exePath

                let (resultState, _) = updateWithServices message initialState

                resultState.Conf.ExecutablesPath |> Expect.equal "Le répertoire est dans le nouvel état" exePath

                ModelTests.cleanSaveFile saveFile

            testCase "Cas Cancel : l'utilisateur a annulé son choix" <| fun _ ->
                let saveFile = ModelTests.generateSaveFileName ()
                let initialState = ModelTests.initTestState saveFile "" "" ""
                let exePath = @"C:\Exe\Path"
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                let message = Msg.ExecutablesPathChosen exePath

                let (resultState, _) = updateWithServices message initialState

                resultState.Conf.ExecutablesPath |> Expect.equal "Le répertoire est l'ancien" exePath

                ModelTests.cleanSaveFile saveFile
        ]

