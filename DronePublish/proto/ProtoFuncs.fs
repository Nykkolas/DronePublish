namespace DronePublish.proto

open System.Threading

type AsyncStatus =
    | STOPPED
    | STARTED of int
    | FINISHED

type AsyncStatusEvent () =
    let evt = new Event<_> ()
    member this.StatusEvent = evt.Publish
    member this.Finish = evt.Trigger FINISHED

module ProtoFuncs =
    let startTaskAndWait message task =
        let waitScreen (entete:string) =
            async {
                printfn "%s" entete
                while true do
                    do! Async.Sleep(500)
                    printfn "."
            }

        let startTask (evt:AsyncStatusEvent) taskFunc =
            async {
                let! taskResult = taskFunc |> Async.AwaitTask
                evt.Finish
                return taskResult
            }

        let jobEvent = AsyncStatusEvent ()

        async {
            use cts = new CancellationTokenSource ()
            
            use disposableJob =
                jobEvent.StatusEvent
                |> Observable.filter (
                    function
                    | FINISHED -> true
                    | _ -> false
                )
                |> Observable.subscribe (fun _ -> cts.Cancel ())

            Async.Start ((waitScreen message), cts.Token)
            let! asyncJob = 
                task 
                |> startTask jobEvent 
                |> Async.StartChild

            return! asyncJob
        } |> Async.RunSynchronously
