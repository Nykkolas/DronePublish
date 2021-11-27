namespace DronePublish.FuncUI.Test

open DronePublish.FuncUI

module DialogsTest =
    let showFolderDialog (result:string) (title:string, directory:string) =
        async {
            return result
        }

    let showSourceFileDialog (result:string) (title:string) =
        async {
            return [| result |]
        }

    let create folderResult sourceFileResult =
        {
            ShowFolderDialog = showFolderDialog folderResult
            ShowSourceFileDialog = showSourceFileDialog sourceFileResult
        }