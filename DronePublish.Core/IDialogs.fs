namespace DronePublish.Core

type IDialogs = {
    ShowFolderDialog: string * string -> Async<string>
    ShowSourceFileDialog: string -> Async<string []>
}
