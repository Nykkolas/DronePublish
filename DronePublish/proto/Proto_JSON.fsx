#r "nuget: FSharp.Json"
open FSharp.Json

type Conf = {
    ExecutablesPath: string
}

type Model = {
    Conf: Conf
    DestDir: string
}

module Proto_JSON =

    let conf = { ExecutablesPath = @"Executable\Path" }

    let model = {
        Conf = conf
        DestDir = @"C:\Dest\Dir"
    }

    let json = Json.serialize model
    printfn "Json :"
    printfn "%s" json
    printfn "======"


    let deserialized = Json.deserialize<Model> json
    printfn "Deserialized :"
    printfn "%A" deserialized
    printfn "=============="



