namespace DronePublish.Core

open System

module Const =
    let saveFile = sprintf @"%s\DronePublish\conf.json" (Environment.GetFolderPath Environment.SpecialFolder.LocalApplicationData)
