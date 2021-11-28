namespace DronePublish.FuncUI

open System
open System.Text.Json
open System.Text.Json.Serialization
open Elmish
open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Controls.ApplicationLifetimes
open Live.Avalonia
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts
open DronePublish.Core

module Program =


    let transferState<'t> oldState =
        let jsonOptions = JsonSerializerOptions()
        jsonOptions.Converters.Add(JsonFSharpConverter())
    
        try
            let json = JsonSerializer.Serialize(oldState, jsonOptions)
            let state = JsonSerializer.Deserialize<'t>(json, jsonOptions)        
            match box state with
            | null -> None
            | _ -> Some state
        with ex ->
            Console.Write $"Error restoring state: {ex}"
            None
    
    let isProduction =
        #if DEBUG
            false
        #else
            true
        #endif
    

//type MainWindow() as this =
//    inherit HostWindow()
//    do
//        base.Title <- "DronePublish.FuncUI"
//        base.Width <- 400.0
//        base.Height <- 400.0
        
//        //this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
//        //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true


//        Elmish.Program.mkSimple MVU.init MVU.update View.view
//        |> Program.withHost this
//        |> Program.run

        
//type App() =
//    inherit Application()

//    override this.Initialize() =
//        this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
//        this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"

//    override this.OnFrameworkInitializationCompleted() =
//        match this.ApplicationLifetime with
//        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
//            desktopLifetime.MainWindow <- MainWindow()
//        | _ -> ()

    type MainControl(window: Window) as this =
        inherit HostControl()
        do
            window.Title <- "DronePublish"
            window.Width <- 600.0
            window.Height <- 400.0
            
            // Instead of just creating default init state, try to recover state from window.DataContext
            let hotInit confFile = 
                match transferState<Model> window.DataContext with
                | Some newState ->
                    Console.WriteLine $"Restored state %O{newState}"
                    (newState, Cmd.none)
                | None -> Model.init confFile
        
#if DEBUG
            window.AttachDevTools(KeyGesture(Key.F12))
#endif
            /// Interface pour permettre de tester
            let dialogs = {
                ShowFolderDialog = Dialogs.showFolderDialog window
                ShowSourceFileDialog = Dialogs.showSourceFileDialog window
                ShowConvertionWindow = Dialogs.showConvertionWindow window
            }

            let updateWithServices msg state =
                Update.update msg state dialogs

            /// Fichier de configuration, passé en paramètre 
            let confFile = sprintf @"%s\DronePublish\conf.json" (Environment.GetFolderPath Environment.SpecialFolder.LocalApplicationData)

            Program.mkProgram hotInit updateWithServices View.view
            |> Program.withHost this
            // Every time state changes, save state to window.DataContext
            |> Program.withTrace (fun _ state -> window.DataContext <- state)
            |> Program.runWith confFile

        
    type App() =
        inherit Application()
    
        interface ILiveView with
            member _.CreateView(window: Window) =
                MainControl(window) :> obj

        override this.Initialize() =
            this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
            this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"

        override this.OnFrameworkInitializationCompleted() =
            match this.ApplicationLifetime with
            | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            
                // Disable live reload in production
                if isProduction then 
                    let window = Window()
                    window.Content <- (this :> ILiveView).CreateView(window)
                    window.Show()
                else
                    let window = new LiveViewHost(this, fun msg -> printfn $"%s{msg}")
                    window.StartWatchingSourceFilesForHotReloading()
                    window.Show()
            
                base.OnFrameworkInitializationCompleted()
            | _ -> ()


    [<EntryPoint>]
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)