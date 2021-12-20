#r "nuget: FsToolkit.ErrorHandling"

open FSharp.Reflection
open FsToolkit.ErrorHandling

type Tel = {
    Nom: string * Result<string,string>
    Numero: string * Result<int,string>
}

type Test1 =    {
    Field1: string
    Field2: int
}

module Proto_Records =
    let fieldToBool (_, result) =
        result |> Result.isOk

    let tel = { Nom = ("Nicolas", Ok "Nicolas"); Numero = ("0603297733", Ok 0603297733) }

    let a =
        FSharpValue.GetRecordFields tel
        |> Array.map (fun e ->
            tryUnbox e
            |> Option.iter (fun s -> 
                match s.GetType() with
                | t when t = typeof<string * Result<string,string>> -> 
                    let f = fst s
                    printfn "Résult " 
                | _ -> ()
            )
        )

    printfn "a : %A" a

    let test = { Field1 = "Du texte"; Field2 = 12345 }

    let affiche s =
        true

    let b =
        FSharpValue.GetRecordFields test
        |> Array.map (fun e ->
            tryUnbox e
            |> Option.map (fun s -> 
                match s.GetType() with
                | t when t = typeof<string> -> affiche s
                | _ -> false
            )
        )

    printfn "b : %A" b