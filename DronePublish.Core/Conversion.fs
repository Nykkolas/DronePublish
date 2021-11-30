namespace DronePublish.Core

open Xabe.FFmpeg
open System.IO
open FsToolkit.ErrorHandling.Operator.Validation

type ConversionError =
    | CantFindFFMpegExe
    | CantFindSourceFile
    | CantFindDestDir
    | DestFileAlreadyExists

type ConversionResult = {
    Duration: string
}

type MediaFileInfos = {
    Width: int
    Height: int
    Codec: string
    Bitrate: int64
}

module Conversion =
    let validateExecutablePath path =
        let ffmpeg = File.Exists (Path.Combine (path, "ffmpeg.exe"))
        let ffprobe = File.Exists (Path.Combine (path, "ffprobe.exe"))
        if (ffmpeg && ffprobe) then
            Ok path
        else
            Error CantFindFFMpegExe

    let validateSourceFile file =
        if (File.Exists file) then
            Ok file
        else
            Error CantFindSourceFile

    let validateDestDir destDir =
        if (Directory.Exists destDir) then
            Ok destDir
        else
            Error CantFindDestDir

    let validateDestFile destFile =
        if File.Exists destFile then
            Error DestFileAlreadyExists
        else
            Ok destFile

    let validateReadiness exePath sourceFile destDir =
        (fun _ _ _ -> ())
        <!^> validateExecutablePath exePath
        <*^> validateSourceFile sourceFile
        <*^> validateDestDir destDir

    let tryStart exePath sourceFile (destDir:string) destFileName =
        let start exePath sourceFile (destDir:string) destFile =
            FFmpeg.SetExecutablesPath exePath

            async {
                let! mediaInfo = 
                    FFmpeg.GetMediaInfo sourceFile
                    |> Async.AwaitTask

                let sourceVideoStream = 
                   mediaInfo.VideoStreams 
                   |> Seq.head
                
                let todoVideoStream = 
                    sourceVideoStream
                        .SetCodec(VideoCodec.h264)
                        .SetSize(VideoSize.Hd1080)
                        .SetBitrate(int64 8000000)
                
                return! 
                    FFmpeg.Conversions.New()
                        .AddStream(todoVideoStream)
                        .SetPixelFormat(PixelFormat.yuv420p)
                        .SetOutput(destFile)
                        .Start()
                    |> Async.AwaitTask
            }
        
        let destFile = Path.Join (destDir, destFileName)

        start 
        <!^> validateExecutablePath exePath
        <*^> validateSourceFile sourceFile
        <*^> validateDestDir destDir
        <*^> validateDestFile destFile

module ConversionResult =
    let create (iConvertionResult:IConversionResult) =
        { Duration = iConvertionResult.Duration.ToString () }

module MediaFileInfos =
    let createWithIMediaInfo (mediaInfo:IMediaInfo) =
        let sourceVideoStream = 
            mediaInfo.VideoStreams 
            |> Seq.head
        { 
            Width = sourceVideoStream.Width 
            Height = sourceVideoStream.Height
            Codec = sourceVideoStream.Codec 
            Bitrate = sourceVideoStream.Bitrate
        }

    let tryReadIMediaInfo exePath sourceFile=
        let readIMediaInfo p f =
            FFmpeg.SetExecutablesPath p
            FFmpeg.GetMediaInfo f

        readIMediaInfo
        <!^> Conversion.validateExecutablePath exePath
        <*^> Conversion.validateSourceFile sourceFile
        