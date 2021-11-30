namespace DronePublish.Core.Test

open System
open System.IO
open Expecto
open Expecto.Flip
open DronePublish.Core
open Elmish
open Xabe.FFmpeg

type ConvertionResultTest () =
    interface IConversionResult with

        member _.get_StartTime () =
            DateTime (07, 11, 1978)

        member _.get_EndTime () =
            DateTime (07, 11, 1978)

        member _.get_Duration () =
            TimeSpan (0, 1, 0)

        member _.get_Arguments () =
            "--help"

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

                let (resultState, resultCmd) = 
                    MediaFileInfos.tryReadIMediaInfo initialState.Conf.ExecutablesPath initialState.SourceFile
                    |> function
                        | Ok t -> 
                            let imediaInfos = t |> Async.AwaitTask |> Async.RunSynchronously
                            updateWithServices (Msg.GotSourceFileInfos (Ok imediaInfos)) initialState
                        | Error _ -> (initialState, Cmd.none)

                resultState |> Expect.equal "Etat avec nouvelles infos" {initialState with SourceInfos = Ok expectedMediaInfos |> Resolved }
                resultCmd |> extractMsg |> Expect.equal "Lancer la sauvegarde de l'état" [| Msg.SaveState |]
        ]

    [<Tests>]
    let startConvertionTests =
        testList "StartConvertion-ConvertionDone" [
            testCase "StartConvertion - erreur : rien ne va" <| fun _ ->
                let initialState = TestHelpers.initTestState "" "" "" ""
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs

                let (resultState, resultCmd) = updateWithServices Msg.StartConvertion initialState

                resultState |> Expect.equal "l'état n'a pas changé" initialState
                resultCmd |> extractMsg |> Expect.equal "Resolved avec erreurs" [| Msg.ConvertionDone (Error [ CantFindFFMpegExe; CantFindSourceFile; CantFindDestDir ]) |]
            
            testCase "ConvertionDone - erreur : rien ne va" <| fun _ ->
                let initialState = TestHelpers.initTestState "" "" "" ""
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs
                let startConvertionErrors = [ CantFindFFMpegExe; CantFindSourceFile; CantFindDestDir ]
                let startConvertionMessage = Msg.ConvertionDone (Error startConvertionErrors)

                let (resultState, resultCmd) = updateWithServices startConvertionMessage initialState

                resultState |> Expect.equal "L'erreur est affichée dans l'état" { initialState with Conversion = Resolved (Error startConvertionErrors) }
                resultCmd |> Expect.isEmpty "Pas de commande"

            testCase "StartConvertion - Rien si la convertion a déjà démarrée" <| fun _ ->
                let initialState = { TestHelpers.initTestState "" @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" @"./Ressources/output" with Conversion = Started }
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs

                let (resultState, resultCmd) = updateWithServices Msg.StartConvertion initialState

                resultState |> Expect.equal "l'état n'a pas changé" initialState
                resultCmd |>  Expect.isEmpty "Pas de message"
                
            testCase "StartConvertion - Cas passant" <| fun _ ->
                let initialState = TestHelpers.initTestState "" @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" @"./Ressources/output"
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs

                let destFile = Path.Join (@"./Ressources/output", "output.mp4")
                if File.Exists destFile then
                    File.Delete destFile

                let (resultState, resultCmd) = updateWithServices Msg.StartConvertion initialState

                resultState |> Expect.equal "L'état est Started" { initialState with Conversion = Started }
                resultCmd |> Expect.isNonEmpty "Il y a une commande de prévue"

            testCase "ConvertionDone - Cas passant" <| fun _ ->
                let initialState = { TestHelpers.initTestState "" @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" @"./Ressources/output" with Conversion = Started }
                let dialogs = DialogsTest.create "" ""
                let updateWithServices message state =
                    Update.update message state dialogs

                let dummyConvertionResult = ConvertionResultTest () :> IConversionResult

                let (resultState, resultCmd) = updateWithServices (Msg.ConvertionDone (Ok dummyConvertionResult))  initialState

                resultState |> Expect.equal "L'état contient le résultat" {initialState with Conversion = Resolved (Ok { Duration = dummyConvertionResult.Duration.ToString () } )}
                resultCmd |> extractMsg |> Expect.equal "On sauvegarde l'état" [| Msg.SaveState |]
        ]
