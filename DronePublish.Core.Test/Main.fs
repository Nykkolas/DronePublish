namespace DronePublish.Core.Test

open Expecto

module Test =
    [<EntryPoint>]
    let main argv =
        Tests.runTestsInAssembly defaultConfig argv
