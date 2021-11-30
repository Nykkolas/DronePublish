namespace DronePublish.Core

open System.Threading.Tasks

type IDialogs = {
    ShowFolderDialog: string * string -> Async<string>
    ShowSourceFileDialog: string -> Async<string []>
    ShowInfoDialog: string -> string -> Async<unit>
}
