namespace DronePublish.FuncUI.Test
open Expecto

module Program =
    [<EntryPoint>]          
    let main argv =
        Tests.runTestsInAssembly defaultConfig argv
