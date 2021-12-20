open FSharp.Reflection
open System

module Proto_Reflection =
    type RecordTest = {
        Premier: int
        Second: int
    }

    let recordType = { Premier = 1; Second = 2 }

    let fields = FSharpType.GetRecordFields typeof<RecordTest>

    let type1 = (fields.[0]).GetType()
        
    
    let field1 = 
        match type1 with
        | t when t = typeof<Int32> -> FSharpValue.GetRecordField (recordType, fields.[0]) :?> int
        | _ -> -1

    //let recordBack = FSharpValue.MakeRecord (typeof<RecordTest>, [|1; 0 |])
    printfn "%A" fields
    printfn "%A" type1
    printfn "%i" field1

   
