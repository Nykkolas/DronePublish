namespace DronePublish.Core

type Deferred<'a> =
    | NotStarted
    | Started
    | Resolved of 'a


