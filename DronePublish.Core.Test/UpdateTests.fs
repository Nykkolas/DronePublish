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
    let exePathTests =
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

    [<Tests>]
    let getSourceFileInfosTests =
        testList "Get/Got-SourceFileInfos" [
            testCase "GetSourceFileInfos : cas passant" <| fun _ ->
                let saveFile = TestHelpers.generateSaveFileName ()
                let initialState = TestHelpers.initTestState saveFile @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" ""
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                
                let (resultState, resultCmd) = updateWithServices Msg.GetSourceFileInfos initialState

                resultState |> Expect.equal "SourceInfos = Started" { initialState with SourceInfos = Started }
            
            testCase "GetSourceFileInfos : manque ffprobe" <| fun _ ->
                let saveFile = TestHelpers.generateSaveFileName ()
                let initialState = TestHelpers.initTestState saveFile @"./Ressources/bin_only_ffmpeg" @"./Ressources/Peniche_Julien_TimeLine_1.mov" ""
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                
                let (resultState, resultCmd) = updateWithServices Msg.GetSourceFileInfos initialState

                resultState |> Expect.equal "Etat initial" initialState
                resultCmd |> extractMsg |> Expect.equal "GotSourceFileInfos Error" [| (GotSourceFileInfos (Error [ CantFindFFMpegExe ])) |]

            testCase "GotSourceFileInfos : cas passant" <| fun _ ->
                let initialState = TestHelpers.initTestState "" @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" ""
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                
                let expectedMediaInfos = {
                    Width = 3840
                    Height = 2160
                    Codec = "dnxhd"
                    Bitrate = int64 1747081078
                }

                let imediaInfos = MediaFileInfos.readIMediaInfo initialState.Conf.ExecutablesPath initialState.SourceFile |> Async.AwaitTask |> Async.RunSynchronously

                let (resultState, resultCmd) = updateWithServices (Msg.GotSourceFileInfos (Ok imediaInfos)) initialState

                resultState |> Expect.equal "Etat avec nouvelles infos" {initialState with SourceInfos = Ok expectedMediaInfos |> Resolved }
                resultCmd |> extractMsg |> Expect.equal "Lancer la sauvegarde de l'état" [| Msg.SaveState |]
        ]

    [<Tests>]
    let startConvertionTests =
        testList "Convertion" [
            testCase "Cas passant" <| fun _ ->
                true |> Expect.isTrue "Vrai est vrai en attendant mieux"
        ]
