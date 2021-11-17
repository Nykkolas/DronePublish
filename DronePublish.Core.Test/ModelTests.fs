namespace DronePublish.Core.Test

open Expecto
open Expecto.Flip
open DronePublish.Core
open System.IO
open System

module ModelTests =
    let initTestState executablesPath destDir =
        { 
            Conf = {
                ExecutablesPath = executablesPath
            }
            DestDir = destDir
        }

    let generateSaveFileName () =
        let r = Random ()
        sprintf @"%s\DronePublish.%i\conf.json" (Environment.GetFolderPath Environment.SpecialFolder.Personal) (r.Next (1, 1000))
        
    let cleanSaveFile saveFile =
        if File.Exists saveFile then
            File.Delete saveFile
        if Directory.Exists (Path.GetDirectoryName saveFile) then
            Directory.Delete (Path.GetDirectoryName saveFile)

    [<Tests>]
    let saveStateToFileTests =
      testList "saveStateToFile" [
        testCase "Crée le répertoire si il n'existe pas" <| fun _ ->
            let saveFile = generateSaveFileName ()
            let testState = initTestState "" ""

            Model.saveStateToFile testState saveFile |> ignore
            let resultDirectoryExists = Directory.Exists (Path.GetDirectoryName saveFile)
            cleanSaveFile saveFile
            resultDirectoryExists |> Expect.isTrue "Le répertoire de sauvegarde existe"
        testCase "Crée le fichier si il n'existe pas" <| fun _ ->
            let saveFile = generateSaveFileName ()
            let testState = initTestState "" ""
            Model.saveStateToFile testState saveFile |> ignore
            let resultFileExists = File.Exists saveFile
            cleanSaveFile saveFile
            resultFileExists |> Expect.isTrue "Le fichier de sauvegarde a été créé"
        testCase "Le contenu du fichier est le bon" <| fun _ ->
            let saveFile = generateSaveFileName ()
            let testState = initTestState "" ""
            let expected = "\
{
  \"Conf\": {
    \"ExecutablesPath\": \"\"
  },
  \"DestDir\": \"\"
}\
"

            Model.saveStateToFile testState saveFile |> ignore

            let result = File.ReadAllText saveFile

            cleanSaveFile saveFile

            result |> Expect.equal "Le contenu du fichier est le bon" expected 
      ]
    
    [<Tests>]
    let testsLoadState =
      testList "loadStateFromFile" [
        testCase "Charge le modèle quand le fichier existe" <| fun _ ->
            let testState = initTestState @"C:\Executables\Path" @"C:\Dest\Dir"
            let saveFile = generateSaveFileName ()
            Model.saveStateToFile testState saveFile |> ignore

            let resultState = Model.loadStateFromFile saveFile

            cleanSaveFile saveFile

            resultState |> Expect.equal "L'état lu est le même que l'initial" (Ok testState)
      ]
