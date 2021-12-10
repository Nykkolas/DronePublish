namespace DronePublish.Core.Test

open System
open System.IO
open Expecto
open Expecto.Flip
open DronePublish.Core
open Elmish

module UpdateTests =
    [<Tests>]
    let exePathTests =
        testList "Répertoire d'exécutables" [
            testCase "Cas passant : état mis à jour" <| fun _ ->
                let saveFile = TestHelpers.generateSaveFileName ()
                let initialState = TestHelpers.initTestState saveFile "" "" ""  List.empty
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
                let initialState = TestHelpers.initTestState saveFile "" "" ""  List.empty
                let exePath = @"C:\Exe\Path"
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                let message = Msg.ExecutablesPathChosen exePath

                let (resultState, resultCmd) = updateWithServices message initialState

                resultState.Conf.ExecutablesPath |> Expect.equal "Le répertoire est l'ancien" exePath
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "La sauvegarde est lancée et les infos mises à jour" [| Msg.SaveState; Msg.GetSourceFileInfos |]

                TestHelpers.cleanSaveFile saveFile
        ]

    [<Tests>]
    let getSourceFileInfosTests =
        testList "Get/Got-SourceFileInfos" [
            testCase "GetSourceFileInfos : cas passant" <| fun _ ->
                let saveFile = TestHelpers.generateSaveFileName ()
                let initialState = TestHelpers.initTestState saveFile @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" ""  List.empty
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                
                let (resultState, resultCmd) = updateWithServices Msg.GetSourceFileInfos initialState

                resultState |> Expect.equal "SourceInfos = Started" { initialState with SourceInfos = Started }
            
            testCase "GetSourceFileInfos : manque ffprobe" <| fun _ ->
                let saveFile = TestHelpers.generateSaveFileName ()
                let initialState = TestHelpers.initTestState saveFile @"./Ressources/bin_only_ffmpeg" @"./Ressources/Peniche_Julien_TimeLine_1.mov" ""  List.empty
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                
                let (resultState, resultCmd) = updateWithServices Msg.GetSourceFileInfos initialState

                resultState |> Expect.equal "Etat initial" initialState
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "GotSourceFileInfos Error" [| (GotSourceFileInfos (Error [ CantFindFFMpegExe ])) |]

            testCase "GotSourceFileInfos : cas passant" <| fun _ ->
                let initialState = TestHelpers.initTestState "" @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" ""  List.empty
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                
                let expectedMediaInfos = {
                    Width = 3840
                    Height = 2160
                    Codec = "dnxhd"
                    Bitrate = int64 1747081078
                }

                let (resultState, resultCmd) = 
                    MediaFileInfos.tryReadIMediaInfo initialState.Conf.ExecutablesPath initialState.SourceFile
                    |> function
                        | Ok t -> 
                            let imediaInfos = t |> Async.AwaitTask |> Async.RunSynchronously
                            updateWithServices (Msg.GotSourceFileInfos (Ok imediaInfos)) initialState
                        | Error _ -> (initialState, Cmd.none)

                resultState |> Expect.equal "Etat avec nouvelles infos" {initialState with SourceInfos = Ok expectedMediaInfos |> Resolved }
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Lancer la sauvegarde de l'état" [| Msg.SaveState |]
        ]

    
