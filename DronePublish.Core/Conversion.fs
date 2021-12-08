namespace DronePublish.Core

open Xabe.FFmpeg
open System.IO
open FsToolkit.ErrorHandling.Operator.Validation

type ConversionResult = {
    Duration: string
}

type MediaFileInfos = {
    Width: int
    Height: int
    Codec: string
    Bitrate: int64
}

type ConversionError =
    | CantFindFFMpegExe
    | CantFindSourceFile
    | CantFindDestDir
    | DestFileAlreadyExists

type ConversionModel = {
    Log: string
    ProfileData: ProfileData array
    Conversions: Deferred<Result<ConversionResult, ConversionError list>> array
}

module Conversion =
    let init conv =
        match conv with
        | Some c -> { c with Log = ""}
        | None ->
            {
                Log = ""
                ProfileData = Array.empty
                Conversions = Array.empty
            }

    let isStarted conv =
        conv.Conversions
        |> Array.exists (function
                | Started -> true
                | _ -> false
            )

    let log text conv =
        { conv with Log = sprintf "%s\n%s" conv.Log text }

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

    let createDestFile destDir (sourceFile:string) profile =
        let sourceFileName = Path.GetFileNameWithoutExtension sourceFile
        sprintf "%s%s" (Path.Join [| destDir; sprintf "%s%s"sourceFileName (NonEmptyString100.unWrap profile.Suffixe) |]) ".mp4"

    let tryStart exePath sourceFile (destDir:string) profile =
        let start (profile:ProfileData) exePath sourceFile (destDir:string) destFile =
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
                        .SetCodec(Profile.convertCodec profile.Codec)
                        .SetSize(PositiveInt.unWrap profile.Width, PositiveInt.unWrap profile.Height)
                        .SetBitrate(PositiveLong.unWrap profile.Bitrate)
                
                return! 
                    FFmpeg.Conversions.New()
                        .AddStream(todoVideoStream)
                        .SetPixelFormat(PixelFormat.yuv420p)
                        .SetOutput(destFile)
                        .Start()
                    |> Async.AwaitTask
            }
        
        let destFile = createDestFile destDir sourceFile profile

        start profile
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
        