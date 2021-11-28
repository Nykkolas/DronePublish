namespace DronePublish.Core

open System.Threading.Tasks

type IDialogs = {
    ShowFolderDialog: string * string -> Async<string>
    ShowSourceFileDialog: string -> Async<string []>
    ShowConvertionWindow: unit -> Async<unit>
}
