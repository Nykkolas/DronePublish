namespace DronePublish.Core

module ArrayFunc =
    let updateAt index value array =
        array
        |> Array.mapi (fun i e -> if i = index then value else e)

