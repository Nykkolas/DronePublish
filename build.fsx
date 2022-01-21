#r "paket:
nuget FSharp.Core >= 5
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target
nuget Fake.Installer.InnoSetup //"

#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Installer
open System

Target.initEnvironment ()

let defaultProject = "DronePublish.FuncUI/DronePublish.FuncUI.fsproj"
let installerFolder = "DronePublish.Installer/Output"

Target.create "Clean" (fun _ ->
    !! "*/bin"
    ++ "*/obj"
    ++ installerFolder
    |> Shell.cleanDirs 
)

Target.create "Build" (fun _ ->
    DotNet.build id defaultProject
)

Target.create "Test" (fun _ ->
    !! "*.Test/*.fsproj"
    |> Seq.iter (DotNet.test id)
)

Target.create "Package" (fun _ ->
    InnoSetup.build (fun p -> 
        { p with
            //Defines = Map ["TAG", Environment.environVarOrDefault "TAG" "fake"]
            Defines = Map ["GIT_TAG_NAME", Environment.environVarOrDefault "TAG" "fake"]
            ScriptFile = "DronePublish.Installer/InstallScript.iss"
            ToolPath = sprintf "%s\Inno Setup 6\iscc.exe" Environment.ProgramFilesX86
        }
    )
)

Target.create "All" ignore

"Clean"
  ==> "Build"
  ==> "Test"
  ==> "Package"
  ==> "All"

Target.runOrDefault "All"
