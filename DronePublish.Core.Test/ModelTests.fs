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
            let testState = TestHelpers.initTestState saveFile "" "" ""
            
            Model.saveStateToFile testState saveFile |> ignore
            let resultDirectoryExists = Directory.Exists (Path.GetDirectoryName saveFile)
            TestHelpers.cleanSaveFile saveFile
            resultDirectoryExists |> Expect.isTrue "Le répertoire de sauvegarde existe"
        testCase "Crée le fichier si il n'existe pas" <| fun _ ->
            let saveFile = TestHelpers.generateSaveFileName ()
            let testState = TestHelpers.initTestState saveFile "" "" ""

            Model.saveStateToFile testState saveFile |> ignore
            let resultFileExists = File.Exists saveFile
            TestHelpers.cleanSaveFile saveFile
            resultFileExists |> Expect.isTrue "Le fichier de sauvegarde a été créé"
        testCase "Le contenu du fichier est le bon" <| fun _ ->
            let saveFile = TestHelpers.generateSaveFileName ()
            let testState = TestHelpers.initTestState saveFile "" "" ""
            let expected = Json.serialize testState

            Model.saveStateToFile testState saveFile |> ignore

            let result = File.ReadAllText saveFile

            TestHelpers.cleanSaveFile saveFile

            result |> Expect.equal "Le contenu du fichier est le bon" expected 
      ]
    
    [<Tests>]
    let testsLoadState =
      testList "loadStateFromFile" [
        testCase "Charge le modèle quand le fichier existe" <| fun _ ->
            let saveFile = TestHelpers.generateSaveFileName ()
            let testState = TestHelpers.initTestState saveFile @"C:\Executables\Path" @"C:\Source\File.mov" @"C:\Dest\Dir"
            Model.saveStateToFile testState saveFile |> ignore

            let resultState = Model.loadStateFromFile saveFile

            TestHelpers.cleanSaveFile saveFile

            resultState |> Expect.equal "L'état lu est le même que l'initial" (Ok testState)
      ]
