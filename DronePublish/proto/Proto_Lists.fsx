
module Proto_Lists =
    let l = [ 1; 2; 3 ]

    let index = 1

    let l' = 
        l 
        |> List.mapi (fun i el -> (i <> index, el))
        |> List.filter (fun (b, _) -> b = true)
        |> List.map (fun (b, e) -> e)

    printfn "%A" l'

    let updateAt index value source =
        source
        |> List.mapi (fun i el -> if i = index then value else el)

    printfn "%A" (l |> updateAt 1 10)
