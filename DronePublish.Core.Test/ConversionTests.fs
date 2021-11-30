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
                let destFileName = @"output tryStartTests Passant.mp4"
                
                let destFile = Path.Join (destDir, destFileName)
                if File.Exists (destFileName) then
                    File.Delete destFile

                Conversion.tryStart @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" destDir destFileName
                |> function
                    | Ok c -> c |> Async.RunSynchronously |> ignore
                    | Error _ -> ()

                File.Exists (destFile) |> Expect.isTrue "Le fichier output existe" 

                if File.Exists (destFile) then
                    File.Delete destFile

            testCase "Le fichier destination existe déjà" <| fun _ ->
                let destDir = @"./Ressources/output"
                let destFileName = @"output tryStartTests Existe.mp4"
                let destFile = Path.Join (destDir, destFileName)
                
                if not (File.Exists destFile) then
                    (File.Create(destFile)).Dispose() |> ignore

                let result = Conversion.tryStart @"./Ressources/bin" @"./Ressources/Peniche_Julien_TimeLine_1.mov" destDir destFileName

                result |> Expect.wantError "Une erreure est remontée" |> Expect.equal "L'erreur est pertinente" [ DestFileAlreadyExists ]

                if File.Exists (destFile) then
                    File.Delete destFile
            
            (* TODO : Génération du nom de fichier destination sur la base du profile *)

            (* TODO :  Utilisation du profile pour déterminer les paramètres de conversion *)
        ]

