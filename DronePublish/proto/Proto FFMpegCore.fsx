#r "nuget: FFMpegCore"
#load "ProtoFuncs.fs"
#load "../Types.fs"
#load "../Fichiers.fs"

// Profile 1080 8Mbps
// .\ffmpeg.exe -i .\Peniche_Julien_TimeLine_1.mov -pix_fmt yuv420p -preset medium -profile:v high -c:v libx264 -b:v 8m -vf scale=1920:1080 .\output\test.mp4

open System
open System.IO
open FFMpegCore
open FFMpegCore.Enums
open FFMpegCore.Arguments
open Types
open ProtoFuncs
open Fichiers

let DNxHRTestFile = @"C:\Users\EliteBook\source\repos\DronePublish\FichiersTest\Peniche_Julien_TimeLine_1.mov"
let destDir = @"C:\Users\EliteBook\source\repos\DronePublish\FichiersTest\output"
let suffixe = "TEST"
let ffOptions = FFOptions (BinaryFolder = @"C:\Users\EliteBook\source\repos\DronePublish\FichiersTest\bin") 
GlobalFFOptions.Configure ffOptions

module Proto_FFMpegCore =
    let filterAction =
        Action<VideoFilterOptions> (fun filterOptions ->
            filterOptions
                .Scale(VideoSize.FullHd)
            |> ignore
        )

    let outputToFileAction = 
        Action<FFMpegArgumentOptions> (fun options ->
            options
                .WithVideoCodec(VideoCodec.LibX264)
                .ForcePixelFormat("yuv420p")
                .WithVideoFilters(filterAction)
            |> ignore
        )
    
    let destFile = createDestFileName destDir DNxHRTestFile suffixe
    
    
    if File.Exists destFile then
        File.Delete destFile

    let conversion = 
        FFMpegArguments
            .FromFileInput(DNxHRTestFile)
            .OutputToFile(destFile, false, outputToFileAction)
            .ProcessAsynchronously()
        |> startTaskAndWait "Conversion"
    
    printfn "=====> Résultats <====="

    printfn "Répertoire d'exécutable : %s" ffOptions.BinaryFolder
    printfn "Répertoire temporaire : %s" ffOptions.TemporaryFilesFolder

    printfn "==> Nom du fichier correctement généré"
    printfn "Fichier destination : %s" destFile
    printfn "==> Fichier destination existe"
    let destFileExists = File.Exists destFile
    printfn "Le fichier existe : %b" destFileExists
    if destFileExists then
        printfn "==> Codec, résolution, bitrate et format de pixel correctes"
        let destMediaInfo = 
             FFProbe.AnalyseAsync destFile
            |> startTaskAndWait "FFProbe fichier source"

        let destVideoStream = 
           destMediaInfo.VideoStreams 
           |> Seq.head

        printfn "Codec : %s" destVideoStream.CodecName
        printfn "WidthxHeight : %ix%i" destVideoStream.Width destVideoStream.Height
        printfn "Bitrate : %i" (int destVideoStream.BitRate)
        printfn "Pixel Format : %s" destVideoStream.PixelFormat

    printfn "======================="
    
    0
