namespace DronePublish.Core.Test

open Expecto
open Expecto.Flip
open DronePublish.Core
open Elmish

module UpdateTests =
    let extractMsg cmd =
        let mutable msgArray = Array.empty
        let dispatch msg =
            msgArray <- Array.append msgArray [| msg |]
            ()
        cmd |> List.iter (fun call -> call dispatch)
        msgArray

    [<Tests>]
    let tests =
        testList "Répertoire d'exécutables" [
            testCase "Cas passant : état mis à jour" <| fun _ ->
                let saveFile = TestHelpers.generateSaveFileName ()
                let initialState = TestHelpers.initTestState saveFile "" "" ""
                let exePath = @"C:\Exe\Path"
                let dialogs = DialogsTest.create exePath ""
                let updateWithServices message state =
                    Update.update message state dialogs
                let message = Msg.ExecutablesPathChosen exePath

                let (resultState, _) = updateWithServices message initialState

                resultState.Conf.ExecutablesPath |> Expect.equal "Le répertoire est dans le nouvel état" exePath
                
                TestHelpers.cleanSaveFile saveFile

            testCase "Cas Cancel : l'utilisateur a annulé son choix" <| fun _ ->
                let saveFile = TestHelpers.generateSaveFileName ()
                let initialState = TestHelpers.initTestState saveFile "" "" ""
                let exePath = @"C:\Exe\Path"
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                let message = Msg.ExecutablesPathChosen exePath

                let (resultState, resultCmd) = updateWithServices message initialState

                resultState.Conf.ExecutablesPath |> Expect.equal "Le répertoire est l'ancien" exePath
                resultCmd |> extractMsg |> Expect.equal "La sauvegarde est lancée et les infos mises à jour" [| Msg.SaveState; Msg.GetSourceFileInfos |]

                TestHelpers.cleanSaveFile saveFile
        ]

