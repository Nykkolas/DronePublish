namespace DronePublish.Core

open System.IO

module Fichiers =
    let createDestFileName destDir (sourceFile:string) suffixe =
        sprintf @"%s\%s %s%s" destDir (Path.GetFileNameWithoutExtension sourceFile) suffixe ".mp4"
        // sprintf @"%s\%s %s%s" destDir (Path.GetFileNameWithoutExtension sourceFile) suffixe (Path.GetExtension sourceFile)

