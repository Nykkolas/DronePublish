namespace DronePublish.Core

open Xabe.FFmpeg

type MediaFileInfos = {
    Width: int
    Height: int
    Codec: string
    Bitrate: int64
}

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

    let readIMediaInfo p f =
        FFmpeg.SetExecutablesPath p
        FFmpeg.GetMediaInfo f
