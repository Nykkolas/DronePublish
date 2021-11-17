open System
open System.Threading

module Proto_Async =
    type AsyncStatus =
        | STOPPED
        | STARTED of int
        | FINISHED

    type AsyncStatusEvent () =
        let evt = new Event<_> ()
        member this.StatusEvent = evt.Publish
        member this.Finish = FINISHED |> evt.Trigger

    let waitScreen (entete:string) =
        async {
            printfn "%s" entete
            while true do
                do! Async.Sleep(500)
                printfn "."
        }

    let attendre (evt:AsyncStatusEvent) =
        async {
            do! Async.Sleep (2000)
            evt.Finish
        }

    let jobEvent = AsyncStatusEvent ()

    async {
        let cts = new CancellationTokenSource ()

        let! jobAttendre = attendre jobEvent |> Async.StartChild
        Async.Start ((waitScreen "Attendre"), cts.Token)

        jobEvent.StatusEvent
        |> Observable.filter (fun s -> 
            match s with
            | FINISHED -> true
            | _ -> false)
        |> Observable.add (fun _ -> cts.Cancel ())

        let! resultAttendre = jobAttendre


        printfn "====="
    } |> Async.RunSynchronously
    
    
