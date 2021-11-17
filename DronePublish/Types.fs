namespace DronePublish

open System.IO

type Conf = {
    ExecutablesPath: string
    DestDir: string
}

type Source = string // fichier

type Destination = string array // Liste des fichiers générés dans le répertoire de destination

type ProfileData = {
    Nom: string
    Suffixe: string
    Bitrate: int64
    Width: uint
    Height: uint
}

type Profile =
    | SELECTED of ProfileData
    | NOTSELECTED of ProfileData

type Profiles = Profile array

module Profiles =
    let selectedProfiles profiles =
        profiles
        |> Array.filter (
            function 
            | SELECTED _ -> true
            | NOTSELECTED _ -> false
        )
