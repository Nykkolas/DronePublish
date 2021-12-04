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
                let profile = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_TESTS"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }

                let result = 
                    match Conversion.tryStart "" "" "" profile with
                    | Ok _ -> []
                    | Error e -> e

                result |> Expect.equal "C'est une erreur" [CantFindFFMpegExe; CantFindSourceFile; CantFindDestDir]

            testCase "Cas passant" <| fun _ ->
                let destDir = @"./Ressources/output"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let profile = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_TESTS"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let destFile = Conversion.createDestFile destDir sourceFile profile

                if File.Exists (destFile) then
                    File.Delete destFile

                Conversion.tryStart @"./Ressources/bin" sourceFile destDir profile
                |> function
                    | Ok c -> c |> Async.RunSynchronously |> ignore
                    | Error _ -> ()

                File.Exists (destFile) |> Expect.isTrue "Le fichier output existe" 

                if File.Exists (destFile) then
                    File.Delete destFile

            testCase "Le fichier destination existe déjà" <| fun _ ->
                let destDir = @"./Ressources/output"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let profile = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_TESTS_Existe"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }

                let destFile = Conversion.createDestFile destDir sourceFile profile
                
                if not (File.Exists destFile) then
                    (File.Create(destFile)).Dispose() |> ignore

                let result = 
                    Conversion.tryStart 
                        @"./Ressources/bin" 
                        @"./Ressources/Peniche_Julien_TimeLine_1.mov" 
                        destDir 
                        profile

                result 
                |> Expect.wantError "Une erreure est remontée" 
                |> Expect.equal "L'erreur est pertinente" [ DestFileAlreadyExists ]

                if File.Exists (destFile) then
                    File.Delete destFile
        ]

    [<Tests>]
    let createDestFileTests =
        testList "createDestFileTests" [
            testCase "Cas passant" <| fun _ ->
                let destDir = @"./Ressources/output"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let profile = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_TESTS"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }

                Conversion.createDestFile destDir sourceFile profile
                |> Expect.equal "création du nom de fichier" @"./Ressources/output\Peniche_Julien_TimeLine_1_TESTS.mp4"
        ]

