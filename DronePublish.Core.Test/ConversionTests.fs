namespace DronePublish.Core.Test

open Expecto
open Expecto.Flip
open DronePublish.Core
open System.IO
open Xabe.FFmpeg

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

    [<Tests>]
    let multiProfilesTests =
        testList "Conversion avec multiples profiles" [
            testCase "Cas passant avec 1 profile - StartConversionJobs" <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @"./Ressources/bin"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let destDir = @"./Ressources/output"
                let profileData = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 1"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = TestHelpers.initTestState "" exePath sourceFile destDir [ Selected profileData ]
                
                (* Action *)
                let (resultState, resultCmd) = Update.update StartConversionJobs initialState dialogs

                (* Expected *)
                let expectedConvertionJobs = {
                    Log = "Conversion de 1 profile(s) démarré"
                    SelectedProfileData = [| profileData |]
                    JobsResults = [| NotStarted |]
                }

                let expectedMsg = StartConversionJob 0
                
                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Les jobs ont été initialisés" expectedConvertionJobs
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "La commande suivante démarre le premier job" [| expectedMsg |]


            testCase "Cas passant avec 1 profile - StartConversionJob 0" <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @"./Ressources/bin"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let destDir = @"./Ressources/output"
                let profileData = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 1"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = {
                    TestHelpers.initTestState "" exePath sourceFile destDir [ Selected profileData ] with
                        ConversionJobs = {
                            Log = "Conversion de 1 profile(s) démarré"
                            SelectedProfileData = [| profileData |]
                            JobsResults = [| NotStarted |]
                        }
                }
                
                (* Action *)
                let (resultState, resultCmd) = Update.update (StartConversionJob 0) initialState dialogs

                (* Expected *)
                let expectedConvertionJobs = {
                    initialState.ConversionJobs with
                        Log = "Conversion de 1 profile(s) démarré\nDémarrage du profile 1/1"
                        JobsResults = [| Started |]
                }

                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Les jobs ont été mis à jour" expectedConvertionJobs
                resultCmd |> Expect.isNonEmpty "Il y a une commande"

            testCase "Cas passant avec 1 profile - ConversionJobDone (0, result) " <| fun _ ->
                (* Conditions initiales *)
                 (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @"./Ressources/bin"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let destDir = @"./Ressources/output"
                let profileData = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 1"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = {
                    TestHelpers.initTestState "" exePath sourceFile destDir [ Selected profileData ] with
                        ConversionJobs = {
                            Log = "Conversion de 1 profile(s) démarré\nDémarrage du profile 1/1"
                            SelectedProfileData = [| profileData |]
                            JobsResults = [| Started |]
                        }
                }
                let dummyConvertionResult = ConvertionResultTest () :> IConversionResult

                (* Action *)
                let (resultState, resultCmd) = Update.update (ConversionJobDone (0, Ok dummyConvertionResult)) initialState dialogs

                (* Expected *)
                let expectedConvertionJobs = {
                    initialState.ConversionJobs with
                        Log = "Conversion de 1 profile(s) démarré\nDémarrage du profile 1/1\nConversion du profile 1/1 terminée"
                        JobsResults = [| Resolved (Ok { Duration = dummyConvertionResult.Duration.ToString () } ) |]
                }
                let expectedMsg = SaveState

                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Les jobs ont été mis à jour" expectedConvertionJobs
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Sauvegarde de l'état" [| expectedMsg |]

            testCase "Cas passant avec 2 profiles - ConversionJobDone (0, result) " <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @"./Ressources/bin"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let destDir = @"./Ressources/output"
                let profileData1 = {
                    Nom = NonEmptyString100 "Pour les tests - p1"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 2"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let profileData2 = {
                    Nom = NonEmptyString100 "Pour les tests - p2"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 2"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = {
                    TestHelpers.initTestState "" exePath sourceFile destDir [ Selected profileData1; Selected profileData2 ] with
                        ConversionJobs = {
                            Log = "Conversion de 1 profile(s) démarré\nDémarrage du profile 1/2"
                            SelectedProfileData = [| profileData1; profileData2 |]
                            JobsResults = [| Started; NotStarted |]
                        }
                }
                let dummyConvertionResult = ConvertionResultTest () :> IConversionResult

                (* Action *)
                let (resultState, resultCmd) = Update.update (ConversionJobDone (0, Ok dummyConvertionResult)) initialState dialogs

                (* Expected *)
                let expectedConvertionJobs = {
                    initialState.ConversionJobs with
                        Log = "Conversion de 1 profile(s) démarré\nDémarrage du profile 1/2\nConversion du profile 1/2 terminée"
                        JobsResults = [| Resolved (Ok { Duration = dummyConvertionResult.Duration.ToString () } ); NotStarted |]
                }
                let expectedMsg = StartConversionJob 1

                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Les jobs ont été mis à jour" expectedConvertionJobs
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Passage au profile suivant" [| expectedMsg |]

            testCase "Aucun profile sélectionné - StartConversionJobs" <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @"./Ressources/bin"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let destDir = @"./Ressources/output"
                let profileData = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 1"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = TestHelpers.initTestState "" exePath sourceFile destDir [ NotSelected profileData ]
                
                (* Action *)
                let (resultState, resultCmd) = Update.update StartConversionJobs initialState dialogs

                (* Expected *)
                let expectedConvertionJobs = {
                    Log = "Aucun profile sélectionné"
                    SelectedProfileData = Array.empty
                    JobsResults = Array.empty
                }
                
                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Les tableaux sont vides" expectedConvertionJobs
                resultCmd |> Expect.isEmpty "Pas de commande"

            testCase "Aucun profile - StartConversionJobs" <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @"./Ressources/bin"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let destDir = @"./Ressources/output"
                let initialState = TestHelpers.initTestState "" exePath sourceFile destDir List.empty
                
                (* Action *)
                let (resultState, resultCmd) = Update.update StartConversionJobs initialState dialogs

                (* Expected *)
                let expectedConvertionJobs = {
                    Log = "Aucun profile sélectionné"
                    SelectedProfileData = Array.empty
                    JobsResults = Array.empty
                }
                
                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Les tableaux sont vides" expectedConvertionJobs
                resultCmd |> Expect.isEmpty "Pas de commande"

            testCase "La conversion a déjà démarré - StartConversionJobs" <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @"./Ressources/bin"
                let sourceFile = @"./Ressources/Peniche_Julien_TimeLine_1.mov"
                let destDir = @"./Ressources/output"
                let profileData = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 1"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = {
                    TestHelpers.initTestState "" exePath sourceFile destDir [ Selected profileData ] with
                        ConversionJobs = {
                            Log = "Conversion de 1 profile(s) démarré"
                            SelectedProfileData = [| profileData |]
                            JobsResults = [| Started |]
                        }
                }
                
                (* Action *)
                let (resultState, resultCmd) = Update.update StartConversionJobs initialState dialogs
                
                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Pas de changement d'état" initialState.ConversionJobs
                resultCmd |> Expect.isEmpty "Pas de commande"

            testCase "Erreur dans les paramètres de conversion - StartConversionJob 0" <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @""
                let sourceFile = @""
                let destDir = @""
                let profileData = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 1"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let initialState = {
                    TestHelpers.initTestState "" exePath sourceFile destDir [ Selected profileData ] with
                        ConversionJobs = {
                            Log = "Conversion de 1 profile(s) démarré"
                            SelectedProfileData = [| profileData |]
                            JobsResults = [| NotStarted |]
                        }
                }
                
                (* Action *)
                let (resultState, resultCmd) = Update.update (StartConversionJob 0) initialState dialogs

                (* Expected *)
                let errors = [CantFindFFMpegExe; CantFindSourceFile; CantFindDestDir]
                let expectedConvertionJobs = { 
                    initialState.ConversionJobs with
                        Log = sprintf "Conversion de 1 profile(s) démarré\nErreur lors du démarrage du profile 1/1 : %A" errors
                }
                let expectedMsg = ConversionJobDone (0, Error errors)

                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Le log a été mis à jour" expectedConvertionJobs
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "ConversionJobDone avec erreur" [| expectedMsg |]

            testCase "Erreur dans les paramètres de conversion (un profile) - ConversionJobDone (0, result) " <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @""
                let sourceFile = @""
                let destDir = @""
                let profileData = {
                    Nom = NonEmptyString100 "Pour les tests"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 1"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let errors = [CantFindFFMpegExe; CantFindSourceFile; CantFindDestDir]
                let initialState = {
                    TestHelpers.initTestState "" exePath sourceFile destDir [ Selected profileData ] with
                        ConversionJobs = {
                            Log = sprintf "Conversion de 1 profile(s) démarré\nErreur lors du démarrage du profile 1/1 : %A" errors
                            SelectedProfileData = [| profileData |]
                            JobsResults = [| NotStarted |]
                        }
                }
                
                (* Action *)
                let (resultState, resultCmd) = Update.update (ConversionJobDone (0, Error errors)) initialState dialogs

                (* Expected *)
                let expectedConvertionJobs = {
                    initialState.ConversionJobs with
                        JobsResults = [| Resolved (Error errors) |]
                }
                let expectedMsg = SaveState

                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Les jobs ont été mis à jour" expectedConvertionJobs
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Sauvegarde de l'état" [| expectedMsg |]

            testCase "Erreur dans les paramètres de conversion du premier - ConversionJobDone (0, result) " <| fun _ ->
                (* Conditions initiales *)
                let dialogs = DialogsTest.create "" ""
                let exePath = @""
                let sourceFile = @""
                let destDir = @""
                let profileData1 = {
                    Nom = NonEmptyString100 "Pour les tests - p1"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 2"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let profileData2 = {
                    Nom = NonEmptyString100 "Pour les tests - p2"
                    Suffixe = NonEmptyString100 "_MULTICONVTESTS Cas 2"
                    Bitrate = PositiveLong 8000000L
                    Width = PositiveInt 1920
                    Height = PositiveInt 1080
                    Codec = H264
                }
                let errors = [DestFileAlreadyExists]
                let initialState = {
                    TestHelpers.initTestState "" exePath sourceFile destDir [ Selected profileData1; Selected profileData2 ] with
                        ConversionJobs = {
                            Log = sprintf "Conversion de 1 profile(s) démarré\nErreur lors du démarrage du profile 1/1 : %A" errors
                            SelectedProfileData = [| profileData1; profileData2 |]
                            JobsResults = [| NotStarted; NotStarted |]
                        }
                }
                
                (* Action *)
                let (resultState, resultCmd) = Update.update (ConversionJobDone (0, Error errors)) initialState dialogs

                (* Expected *)
                let expectedConvertionJobs = {
                    initialState.ConversionJobs with
                        JobsResults = [| Resolved (Error errors); NotStarted |]
                }
                let expectedMsg = StartConversionJob 1

                (* Tests *)
                resultState.ConversionJobs |> Expect.equal "Les jobs ont été mis à jour" expectedConvertionJobs
                resultCmd |> TestHelpers.extractMsg |> Expect.equal "Job suivant" [| expectedMsg |]
        ]

