#r "nuget: FSharp.Json"
open FSharp.Json
open System
open System.IO

let appConfFolder = "DronePublish"
let confFile = "conf-proto.json"

type Conf = {
    ExecutablesPath: string
}

type State = {
    Conf: Conf
    DestDir: string
}

module Proto_Conf =
    let conf = { ExecutablesPath = @"C:\Users\EliteBook\source\repos\DronePublish\FichiersTest\bin" }
    let state = { Conf = conf; DestDir = @"C:\Users\EliteBook\source\repos\DronePublish\FichiersTest\output" }

    let serializedState = Json.serialize state

    let saveFolder = sprintf @"%s\%s" (Environment.GetFolderPath Environment.SpecialFolder.LocalApplicationData) appConfFolder
    let saveFile = sprintf @"%s\%s" saveFolder confFile
    
    if not (Directory.Exists saveFolder) then
        Directory.CreateDirectory saveFolder |> ignore
        (* Ajouter test -> si création impossible, informer et quitter quand même *)

    File.WriteAllText (saveFile, serializedState)
    (* Ajouter test -> si création impossible, informer et quitter quand meme *)

    printfn "== Est-ce que le répertoire existe ?"
    printfn "%b" (Directory.Exists saveFolder)
    printfn "===================================="
    
    printfn "== Fichier de sauvegarde"
    printfn @"%s" saveFile
    printfn "========================"
    
    printfn "== Est-ce que le fichier existe ?"
    let saveFileExists = File.Exists saveFile
    printfn "%b" saveFileExists
    if saveFileExists then 
        printfn "Contenu :"
        printfn "%s" (File.ReadAllText saveFile)
        File.Delete saveFile
        printfn "Est-ce que le fichier existe toujours ?"
        printfn "%b" (File.Exists saveFile)
    printfn "================================="
    