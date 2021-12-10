namespace DronePublish.Core.Test

open DronePublish.Core
open System
open System.IO
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

module TestHelpers =
    let initTestState confFile executablesPath sourceFile destDir profileList =
        { 
            ConfFile = confFile
            Conf = {
                ExecutablesPath = executablesPath
            }
            SourceFile = sourceFile
            SourceInfos = NotStarted
            DestDir = destDir
            Conversion = NotStarted
            ConversionJobs = ConversionJobs.init ()
            Profiles = profileList
        }

    let generateSaveFileName () =
        let r = Random ()
        sprintf @"%s\DronePublish.%i\conf.json" (Environment.GetFolderPath Environment.SpecialFolder.Personal) (r.Next (1, 1000))
    
    let cleanSaveFile saveFile =
        if File.Exists saveFile then
            File.Delete saveFile
        if Directory.Exists (Path.GetDirectoryName saveFile) then
            Directory.Delete (Path.GetDirectoryName saveFile)

    let extractMsg cmd =
        let mutable msgArray = Array.empty
        let dispatch msg =
            msgArray <- Array.append msgArray [| msg |]
            ()
        cmd |> List.iter (fun call -> call dispatch)
        msgArray

