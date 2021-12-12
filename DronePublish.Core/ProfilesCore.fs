namespace DronePublish.Core

open Elmish

module ProfilesCore =
    type Model = Profile list
    
    type Msg = 
        | EditProfile of int
        | ProfileEdited of (int * Profile)
        | DeleteProfile of int
        | CheckProfile of int
        | UnCheckProfile of int
    
    type ExternalMsg =
        | SaveState

    let init () =
        List.empty
    
    let update msg (state:Model) dialogs =
        match msg with
        | EditProfile index ->
            match index with
            | -1 -> (state, Cmd.OfAsync.perform dialogs.ShowEditProfileDialog (index, Empty) ProfileEdited, None)
            | _ -> (state, Cmd.OfAsync.perform dialogs.ShowEditProfileDialog (index, state.[index]) ProfileEdited, None)
        
        | ProfileEdited (index, newProfile) ->
            match newProfile with
            | Empty -> (state, Cmd.none, None)
            | _ -> 
                match index with
                | -1 -> ( state @ [ newProfile ], Cmd.none, Some SaveState )
                | _ -> 
                    let newProfiles = state |> Profile.updateAt index newProfile
                    (newProfiles, Cmd.none, Some SaveState)
            
        | DeleteProfile index ->
            let newProfileList = state |> Profile.removeAt index
            (newProfileList, Cmd.none, Some SaveState)
                        
        | CheckProfile index ->
            let newProfiles =
                state
                |> List.mapi (fun i el -> if i = index then Profile.select el else el)
            (newProfiles, Cmd.none, Some SaveState)
            
        | UnCheckProfile index ->
            let newProfiles =
                state
                |> List.mapi (fun i el -> if i = index then Profile.unSelect el else el)
            (newProfiles, Cmd.none, Some SaveState)
    
        
