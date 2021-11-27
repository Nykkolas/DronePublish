namespace DronePublis.FuncUI.Test

module Sample =

    open Expecto

    [<Tests>]
    let tests =
      testList "Expecto is OK" [
        testCase "universe exists (╭ರᴥ•́)" <| fun _ ->
          let subject = true
          Expect.isTrue subject "I compute, therefore I am."
      ]

