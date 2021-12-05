open FSharp.Reflection
open System

type Tel = {
    Nom: string
    Numero: int
}

module Proto_Records =
    let tel = { Nom = "Nicolas"; Numero = 0603297733 }

    let a =
        FSharpType.GetRecordFields typeof<Tel>
        |> Array.map (fun e -> e.GetValue(tel))
        |> List.ofArray
    
    printfn "%A" a
