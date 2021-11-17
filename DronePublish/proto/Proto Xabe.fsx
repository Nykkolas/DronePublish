#r "nuget: Xabe.FFMpeg"
#load "ProtoFuncs.fs"
#load "../Types.fs"
#load "../Fichiers.fs"

// Profile 1080 8Mbps
// .\ffmpeg.exe -i .\Peniche_Julien_TimeLine_1.mov -pix_fmt yuv420p -preset medium -profile:v high -c:v libx264 -b:v 8m -vf scale=1920:1080 .\output\test.mp4

open System.IO
open Xabe.FFmpeg
// open Types
open DronePublish.proto.ProtoFuncs
open DronePublish.Fichiers

let DNxHRTestFile = @"C:\Users\EliteBook\source\repos\DronePublish\FichiersTest\Peniche_Julien_TimeLine_1.mov"
let destDir = @"C:\Users\EliteBook\source\repos\DronePublish\FichiersTest\output"
let suffixe = "TEST"
FFmpeg.SetExecutablesPath @"C:\Users\EliteBook\source\repos\DronePublish\FichiersTest\bin"

module Proto_Xabe =
    let mediaInfo = 
        FFmpeg.GetMediaInfo DNxHRTestFile
        |> startTaskAndWait "GetMediaInfo fichier source"

    let sourceVideoStream = 
       mediaInfo.VideoStreams 
       |> Seq.head
    
    let todoVideoStream = 
        sourceVideoStream
            .SetCodec(VideoCodec.h264)
            .SetSize(VideoSize.Hd1080)
            .SetBitrate(int64 8000000)
    
    let destFile = createDestFileName destDir DNxHRTestFile suffixe
    
    if File.Exists destFile then
        File.Delete destFile

    let conversion = 
        FFmpeg.Conversions.New()
            .AddStream(todoVideoStream)
            .SetPixelFormat(PixelFormat.yuv420p)
            .SetOutput(destFile)
            
    let conversionObservable = 
        conversion.OnProgress
        |> Observable.subscribe (fun a ->
            printf "[%s / %s] %i%%\r" (a.Duration.ToString ()) (a.TotalLength.ToString ()) a.Percent
        )
    
    // conversion.Start() |> startTaskAndWait "Conversion"
    printfn "Conversion: "
    conversion.Start() |> Async.AwaitTask |> Async.RunSynchronously
    printfn "\n...terminé"

    printfn "=====> Résultats <====="

    printfn "Répertoire avec les exécutables : %s" FFmpeg.ExecutablesPath

    printfn "==> Nom du fichier correctement généré"
    printfn "Fichier destination : %s" destFile
    printfn "==> Fichier destination existe"
    let destFileExists = File.Exists destFile
    printfn "Le fichier existe : %b" destFileExists
    if destFileExists then
        printfn "==> Codec, résolution, bitrate et format de pixel correctes"
        let destMediaInfo = 
            FFmpeg.GetMediaInfo destFile
            |> startTaskAndWait "GetMediaInfo fichier destination"

        let destVideoStream = 
           destMediaInfo.VideoStreams 
           |> Seq.head

        printfn "Codec : %s" destVideoStream.Codec
        printfn "WidthxHeight : %ix%i" destVideoStream.Width destVideoStream.Height
        printfn "Bitrate : %i" (int destVideoStream.Bitrate)
        printfn "Pixel Format : %s" destVideoStream.PixelFormat

    printfn "======================="
    
    0
