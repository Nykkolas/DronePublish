#r "nuget: Elmish"

open Elmish

type Msg =
    | PasDeMessage
    | UnMessage
    | AsyncDone of string

type State = int

module Proto_Elmish_Test =
    let init ():State*Cmd<Msg> = (0, Cmd.none)

    let update msg state =
        match msg with
        | PasDeMessage | AsyncDone _ -> (state, Cmd.none)
        | UnMessage -> (state + 1, Cmd.none)
        
    let view state dispatch =
        dispatch UnMessage
        dispatch UnMessage

    Program.mkProgram init update view
    |> Program.run

    //printfn "***"
    //printfn "%i" 
    //printfn "***"
(*
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
    
*)

