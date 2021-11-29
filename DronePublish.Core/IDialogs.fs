namespace DronePublish.Core

open System.Threading.Tasks

type IDialogs = {
    ShowFolderDialog: string * string -> Async<string>
    ShowSourceFileDialog: string -> Async<string []>
    (* EXEMPLE DE FENETRE MODALE cf ConvertionView.fs/IDialogs.fs/Dialogs.fs/Program.fs/DialogTest.fs *)    
    //ShowConvertionWindow: unit -> Async<unit>
}
