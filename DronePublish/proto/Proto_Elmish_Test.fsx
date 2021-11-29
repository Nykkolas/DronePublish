#r "nuget: Elmish"

open Elmish

type Msg =
    | PasDeMessage
    | UnMessage
    | AsyncDone of string

module Proto_Elmish_Test =
    let asyncFunc arg =
        async {
            do printfn "%s" arg
            return "ok"
        }

    let extractMsg cmd =
        let mutable msgArray = [||]
        let dispatch msg =
            msgArray <- Array.append msgArray [| msg |]
            ()
        cmd |> List.iter (fun call -> call dispatch)
        msgArray

    let extractAsyncMsg cmd =
        let dispatch msg =
            printfn "dispatch"
            ()
        cmd |> List.iter (fun call -> call dispatch)
    //let cmd = Cmd.batch [ Cmd.ofMsg PasDeMessage; Cmd.ofMsg UnMessage ]

    let asyncCmd = Cmd.OfAsync.perform asyncFunc "toto" AsyncDone

    extractAsyncMsg asyncCmd |> ignore

    printfn "***"
    //printfn "%A" (extractAsyncMsg asyncCmd)
    printfn "***"
    


