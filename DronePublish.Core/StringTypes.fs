namespace DronePublish.Core

open System
open FsToolkit.ErrorHandling.Operator.Validation

type StringError =
    | IsNullOrEmpty
    | LengthHigherThan100

type NonEmptyString100 = NonEmptyString100 of string

module NonEmptyString100 =
    let tryCreate s =
        let isNullOrEmpty s =
            if String.IsNullOrEmpty s then
                Error IsNullOrEmpty
            else
                Ok s

        let isLongerThan100 s =
            if String.length s > 100 then
                Error LengthHigherThan100
            else
                Ok s

        (fun _ s -> NonEmptyString100 s)
        <!^> isNullOrEmpty s
        <*^> isLongerThan100 s

    let unWrap nonEmptyS100 =
        let (NonEmptyString100 s) = nonEmptyS100
        s


            

