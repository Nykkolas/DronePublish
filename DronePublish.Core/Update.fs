namespace DronePublish.Core

open Elmish
open Xabe.FFmpeg
open FsToolkit.ErrorHandling

type Msg =
    | ChooseExecutablesPath
    | ExecutablesPathChosen of string
    | ChooseSourceFile
    | SourceFileChosen of string array
    | GetSourceFileInfos
    | GotSourceFileInfos of Result<IMediaInfo,ConversionError list>
    | ChooseDestDir
    | DestDirChosen of string
    | StartConversion
    | ConversionDone of Result<IConversionResult,ConversionError list>
    | StartConversionJobs
    | StartConversionJob of int
    | ConversionJobDone of int * Result<IConversionResult,ConversionError list>
    | SaveState
    | ShowDialog
    | DialogShown
    | EditProfile of int
    | ProfileEdited of (int * Profile)
    | DeleteProfile of int
    | CheckProfile of int
    | UnCheckProfile of int

module Update =
    let update msg state dialogs =
        match msg with
        | ChooseExecutablesPath -> 
            (state, Cmd.OfAsync.perform dialogs.ShowFolderDialog ("Répertoire des binaires", state.Conf.ExecutablesPath) ExecutablesPathChosen)
        
        | ExecutablesPathChosen f ->
            match f with
            | "" -> (state, Cmd.none)
            | _ -> ({ state with Conf = { state.Conf with ExecutablesPath = f } }, Cmd.batch [ Cmd.ofMsg SaveState; Cmd.ofMsg GetSourceFileInfos ])
        
        | ChooseSourceFile ->
            (state, Cmd.OfAsync.perform dialogs.ShowSourceFileDialog state.SourceFile SourceFileChosen)
        
        | SourceFileChosen f ->
            match Array.length f with
            | 0 -> (state, Cmd.none)
            | _ -> ({ state with SourceFile = f.[0] }, Cmd.batch [ Cmd.ofMsg SaveState; Cmd.ofMsg GetSourceFileInfos ])
        
        | GetSourceFileInfos ->
            match state.SourceInfos with
            | Started -> (state, Cmd.none)
            | NotStarted | Resolved _  ->
                MediaFileInfos.tryReadIMediaInfo state.Conf.ExecutablesPath state.SourceFile
                |> function 
                    | Ok c -> 
                        { state with SourceInfos = Started}, 
                        Cmd.OfTask.perform (fun _ -> c) () (Ok >> GotSourceFileInfos)
                    | Error e -> (state, Cmd.ofMsg (GotSourceFileInfos (Error e)))
                
        | GotSourceFileInfos i ->
            let mediaFileInfos =
                match i with
                | Ok media -> MediaFileInfos.createWithIMediaInfo media |> Ok
                | Error e -> Error e
            ({ state with SourceInfos = Resolved mediaFileInfos}, Cmd.ofMsg SaveState)
        
        | ChooseDestDir -> 
            (state, Cmd.OfAsync.perform dialogs.ShowFolderDialog ("Répertoire de destination", state.DestDir) DestDirChosen)
        
        | DestDirChosen f ->
            match f with
            | "" -> (state, Cmd.none)
            | _ -> ({ state with DestDir = f }, Cmd.ofMsg SaveState)
        
        | StartConversion ->
            let profile = {
                Nom = NonEmptyString100 "Pour l'appli"
                Suffixe = NonEmptyString100 "_APP"
                Bitrate = PositiveLong 8000000L
                Width = PositiveInt 1920
                Height = PositiveInt 1080
                Codec = H264
            }

            match state.Conversion with
            | Started -> (state, Cmd.none)
            | Resolved _ | NotStarted ->
                match Conversion.tryStart state.Conf.ExecutablesPath state.SourceFile state.DestDir profile with
                | Ok c -> 
                    ( { state with Conversion = Started }, Cmd.OfAsync.perform (fun _ -> c) () (Ok >> ConversionDone))
                | Error e -> (state, Cmd.ofMsg (ConversionDone (Error e)))
        
        | ConversionDone r ->
            match r with
            | Error e -> ( { state with Conversion = Resolved (Error e) }, Cmd.none)
            | Ok c -> ( { state with Conversion = ConversionResult.create c |> Ok |> Resolved }, Cmd.ofMsg SaveState)
        
        | StartConversionJobs ->
            match (ConversionJobs.isStarted state.ConversionJobs) with
            | true -> (state, Cmd.none)
            | false ->
                let selectedProfileData = 
                    state.Profiles
                    |> Array.ofList
                    |> Array.choose (function
                        | Empty | NotSelected _ -> None
                        | Selected p -> Some p
                    )
                
                let (conversionJobs, cmd) = 
                    match selectedProfileData with
                    | [||] -> 
                        ({
                            Log = "Aucun profile sélectionné"
                            SelectedProfileData = Array.empty
                            JobsResults = Array.empty
                        }, Cmd.none)
                    | _ ->
                        let length = Array.length selectedProfileData
                        let log = sprintf "Conversion de %i profile(s) démarré" length
                        let jobsResults = Array.create length NotStarted
                        ({
                            Log = log
                            SelectedProfileData = selectedProfileData
                            JobsResults = jobsResults
                        }, Cmd.ofMsg (StartConversionJob 0))

                ({ state with ConversionJobs = conversionJobs }, cmd)

        | StartConversionJob index ->
            let (conversionJobs, cmd) =
                match Conversion.tryStart state.Conf.ExecutablesPath state.SourceFile state.DestDir state.ConversionJobs.SelectedProfileData.[index] with
                | Error e -> 
                    let log = sprintf "%s\nErreur lors du démarrage du profile %i/%i : %A" state.ConversionJobs.Log (index + 1) (Array.length state.ConversionJobs.SelectedProfileData) e
                    let conversionJobs = {
                        state.ConversionJobs with
                            Log = log
                    }
                    (conversionJobs, Cmd.ofMsg (ConversionJobDone (index, Error e)))
            
                | Ok c -> 
                    let log = sprintf "%s\nDémarrage du profile %i/%i" state.ConversionJobs.Log (index + 1) (Array.length state.ConversionJobs.SelectedProfileData)
                    let jobsResults = state.ConversionJobs.JobsResults |> ArrayFunc.updateAt index Started
                    let conversionJobs = { 
                        state.ConversionJobs with
                            Log = log
                            JobsResults = jobsResults
                    }
                    let cmd = Cmd.OfAsync.perform (fun _ -> c) () (fun r -> ConversionJobDone (index, Ok r))
                    (conversionJobs, cmd)

            ({ state with ConversionJobs = conversionJobs }, cmd)

        | ConversionJobDone (index, result) ->
            let length = Array.length state.ConversionJobs.SelectedProfileData
            
            let conversionJobs =
                match result with
                | Error e -> 
                    let jobsResults = 
                        state.ConversionJobs.JobsResults 
                        |> ArrayFunc.updateAt index (Resolved (Error e))
                    { state.ConversionJobs with JobsResults = jobsResults }
            
                | Ok r ->
                    let log = sprintf "%s\nConversion du profile %i/%i terminée" state.ConversionJobs.Log (index + 1) length
                    let jobsResults = 
                        state.ConversionJobs.JobsResults 
                        |> ArrayFunc.updateAt index (Resolved (ConversionResult.create r |> Ok))
                    {
                        state.ConversionJobs with
                            Log = log
                            JobsResults = jobsResults
                    }

            let cmd = 
                if (index + 1) >= length 
                then Cmd.ofMsg SaveState
                else Cmd.ofMsg (StartConversionJob (index + 1))

            ({ state with ConversionJobs = conversionJobs }, cmd)

        | SaveState ->
            Model.saveState state
            (state, Cmd.none)

        | ShowDialog ->
            (state, Cmd.OfAsync.perform (fun _ -> dialogs.ShowInfoDialog "Test" "Toto") () (fun _ -> DialogShown))

        | DialogShown ->
            (state, Cmd.none)

        | EditProfile index ->
            match index with
            | -1 -> (state, Cmd.OfAsync.perform dialogs.ShowEditProfileDialog (index, Empty) ProfileEdited)
            | _ -> (state, Cmd.OfAsync.perform dialogs.ShowEditProfileDialog (index, state.Profiles.[index]) ProfileEdited)
            
        | ProfileEdited (index, newProfile) ->
            match newProfile with
            | Empty -> (state, Cmd.none)
            | _ -> 
                match index with
                | -1 -> ( { state with Profiles = state.Profiles @ [ newProfile ] }, Cmd.ofMsg SaveState)
                | _ -> 
                    let newProfiles = state.Profiles |> Profile.updateAt index newProfile
                    ( { state with Profiles = newProfiles }, Cmd.ofMsg SaveState )

        | DeleteProfile index ->
            let newProfileList = state.Profiles |> Profile.removeAt index

            ({ state with Profiles = newProfileList }, Cmd.ofMsg SaveState)
            
        | CheckProfile index ->
            let newProfiles =
                state.Profiles
                |> List.mapi (fun i el -> if i = index then Profile.select el else el)
            ({ state with Profiles = newProfiles }, Cmd.ofMsg SaveState)

        | UnCheckProfile index ->
            let newProfiles =
                state.Profiles
                |> List.mapi (fun i el -> if i = index then Profile.unSelect el else el)
            ({ state with Profiles = newProfiles }, Cmd.ofMsg SaveState)
