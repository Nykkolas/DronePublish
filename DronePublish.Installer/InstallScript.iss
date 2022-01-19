[Setup]
AppName=DronePublish
#ifndef TAG
	AppVersion=latest
#else
	AppVersion={#TAG}
#endif
DefaultDirName={commonpf}\DronePublish
DefaultGroupName=DronePublish
UninstallDisplayIcon={app}\DronePublish.FuncUI.exe
Compression=lzma2
SolidCompression=yes
OutputDir=Output
OutputBaseFilename=DronePublish Installer-{#SetupSetting("AppVersion")}
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\DronePublish.FuncUI\bin\Release\net5.0-windows\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\DronePublish"; Filename: "{app}\DronePublish.FuncUI.exe"		
