#r "nuget: Elmish"

open Elmish

type Msg =
    | PasDeMessage
    | UnMessage

module Proto_Elmish_Test =
    
    let extractMsg cmd =
        let mutable msgArray = [||]
        let dispatch msg =
            msgArray <- Array.append msgArray [| msg |]
            ()
        cmd |> List.iter (fun call -> call dispatch)
        
        msgArray

    let cmd = Cmd.batch [ Cmd.ofMsg PasDeMessage; Cmd.ofMsg UnMessage ]

    printfn "***"
    printfn "%A" (extractMsg cmd)
    printfn "***"
    


