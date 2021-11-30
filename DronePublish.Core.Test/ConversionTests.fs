namespace DronePublish.Core.Tests

open Expecto
open Expecto.Flip
open DronePublish.Core
open System.IO

module ConversionTests =
    [<Tests>]
    let tryStartTests =
        testList "tryStartTests" [
            testCase "Rien ne va" <| fun _ ->
                let result = 
                    match Conversion.tryStart "" "" "" "" with
                    | Ok _ -> []
                    | Error e -> e

                result |> Expect.equal "C'est une erreur" [CantFindFFMpegExe; CantFindSourceFile; CantFindDestDir]

            testCase "Cas passant" <| fun _ ->
                let destDir = @"./Ressources/output"
                let destFileName = @"output tryStartTests Rien.mp4"
                
                Conversion.tryStart @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" destDir destFileName
                |> function
                    | Ok c -> c |> Async.RunSynchronously |> ignore
                    | Error _ -> ()

                let destFile = Path.Join (destDir, destFileName)
                File.Exists (destFile) |> Expect.isTrue "Le fichier output existe" 

                if File.Exists (destFile) then
                    File.Delete destFile

            (* TODO : Erreur lorsque le fichier de destination existe déjà *)

            (* TODO : Génération du nom sur la base du profile *)

            (* TODO :  Utilisation du profile pour déterminer les paramètres de convertion *)
        ]

