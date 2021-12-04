namespace DronePublish.Core.Test

open Expecto
open Expecto.Flip
open DronePublish.Core
open System.IO
open System
open FSharp.Json

module ModelTests =
    [<Tests>]
    let saveStateToFileTests =
      testList "saveStateToFile" [
        testCase "Crée le répertoire si il n'existe pas" <| fun _ ->
            let saveFile = TestHelpers.generateSaveFileName ()
            let testState = TestHelpers.initTestState saveFile "" "" "" List.empty
            
            Model.saveStateToFile testState saveFile |> ignore
            let resultDirectoryExists = Directory.Exists (Path.GetDirectoryName saveFile)
            TestHelpers.cleanSaveFile saveFile
            resultDirectoryExists |> Expect.isTrue "Le répertoire de sauvegarde existe"
        testCase "Crée le fichier si il n'existe pas" <| fun _ ->
            let saveFile = TestHelpers.generateSaveFileName ()
            let testState = TestHelpers.initTestState saveFile "" "" "" List.empty

            Model.saveStateToFile testState saveFile |> ignore
            let resultFileExists = File.Exists saveFile
            TestHelpers.cleanSaveFile saveFile
            resultFileExists |> Expect.isTrue "Le fichier de sauvegarde a été créé"
        testCase "Le contenu du fichier est le bon" <| fun _ ->
            let saveFile = TestHelpers.generateSaveFileName ()
            let testState = TestHelpers.initTestState saveFile "" "" "" List.empty
            let expected = Json.serialize testState

            Model.saveStateToFile testState saveFile |> ignore

            let result = File.ReadAllText saveFile

            TestHelpers.cleanSaveFile saveFile

            result |> Expect.equal "Le contenu du fichier est le bon" expected 
      ]
    
    [<Tests>]
    let testsLoadState =
      testList "loadStateFromFile" [
        testCase "Charge le modèle quand le fichier existe (liste de profiles vide)" <| fun _ ->
            let saveFile = TestHelpers.generateSaveFileName ()
            let testState = TestHelpers.initTestState saveFile @"C:\Executables\Path" @"C:\Source\File.mov" @"C:\Dest\Dir" List.empty
            Model.saveStateToFile testState saveFile |> ignore

            let resultState = Model.loadStateFromFile saveFile

            TestHelpers.cleanSaveFile saveFile

            resultState |> Expect.equal "L'état lu est le même que l'initial" (Ok testState)
        testCase "Charge le modèle quand le fichier existe (liste de profiles existante)" <| fun _ ->
            let existingProfile = NotSelected {
                Nom = NonEmptyString100 "Load state : liste existante"
                Suffixe = NonEmptyString100 "_LoadStateExistsProfile"
                Bitrate = PositiveLong 10000L
                Width = PositiveInt 1920
                Height = PositiveInt 1080
                Codec = H264
            }
            let saveFile = TestHelpers.generateSaveFileName ()
            let testState = TestHelpers.initTestState saveFile @"C:\Executables\Path" @"C:\Source\File.mov" @"C:\Dest\Dir" [ existingProfile ]
            Model.saveStateToFile testState saveFile |> ignore

            let resultState = Model.loadStateFromFile saveFile

            TestHelpers.cleanSaveFile saveFile

            resultState |> Expect.equal "L'état lu est le même que l'initial" (Ok testState)
      ]
