namespace DronePublish.Core.Test

open DronePublish.Core
open Expecto
open Expecto.Flip

module ProfileTests =
    [<Tests>]
    let tryParseBitrateTests =
        testList "tryParseBitrate" [
            testCase "123" <| fun _ ->
                let str = "123"
                let expected = PositiveLong 123L

                Profile.tryParseBitrate str |> Expect.wantOk "Parsing Ok" |> Expect.equal "123" expected

            testCase "123k" <| fun _ ->
                let str = "123k"
                let expected = PositiveLong 123000L

                Profile.tryParseBitrate str |> Expect.wantOk "Parsing Ok" |> Expect.equal "123" expected

            testCase "123M" <| fun _ ->
                let str = "123M"
                let expected = PositiveLong 123000000L

                Profile.tryParseBitrate str |> Expect.wantOk "Parsing Ok" |> Expect.equal "123" expected

            testCase "1 23k (KO)" <| fun _ ->
                let str = "1 23k"
                let expected = CantParseBitrate "Error in Ln: 1 Col: 2\n\
                                                1 23k\n ^\n\
                                                Expecting: end of input, 'M' or 'k'\n"

                Profile.tryParseBitrate str |> Expect.isError "Parsing KO"
        ]

