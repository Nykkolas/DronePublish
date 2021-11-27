namespace DronePublish.Core.Test

open DronePublish.Core
open System
open System.IO


module TestHelpers =
    let initTestState confFile executablesPath sourceFile destDir =
        { 
            ConfFile = confFile
            Conf = {
                ExecutablesPath = executablesPath
            }
            SourceFile = sourceFile
            SourceInfos = NotStarted
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

