namespace DronePublish.Core.Test

open DronePublish.Core

module DialogsTest =
    let showFolderDialog (result:string) (title:string, directory:string) =
        async {
            return result
        }

    let showSourceFileDialog (result:string) (title:string) =
        async {
            return [| result |]
        }

    let showInfoDialog (title:string) (message:string) =
        async {
            return ()
        }

    let showEditProfileDialog indexProfile =
        async {
            return indexProfile
        }

    let create folderResult sourceFileResult =
        {
            ShowFolderDialog = showFolderDialog folderResult
            ShowSourceFileDialog = showSourceFileDialog sourceFileResult
            ShowInfoDialog = showInfoDialog
            ShowEditProfileDialog = showEditProfileDialog
        }