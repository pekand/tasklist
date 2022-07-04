; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

;git describe --tags --abbrev=0
;git push --tags
;git tag v1.0.0
;git tag --list
;git push --delete origin v1.0.0
;git tag -d v1.1.0

#define MyAppName "TaskList"
#define MyAppPublisher "pekand"
#define MyAppURL "http://tasklist.pekand.com/"
#define MyAppExeName "TaskList.exe"

#define GetRevision() \
  Local[0] = "/S /C git describe --tags --abbrev=0 > revision.txt", \
  Local[1] = Exec("cmd.exe", Local[0], SourcePath, , SW_HIDE), \
  Local[2] = FileOpen(AddBackslash(SourcePath) + "revision.txt"), \
  Local[3] = FileRead(Local[2]), \
  FileClose(Local[2]), \
  Local[3] 

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{21831117-41CD-40B4-B1AC-099BC73516C5}
AppName={#MyAppName}
AppVersion={#GetRevision()}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputBaseFilename=tasklist
SetupIconFile=TaskList\TaskList.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "TaskList\bin\Release\TaskList.exe"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKLM; Subkey: "Software\pekand"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\pekand\TaskList"; Flags: uninsdeletekey; ValueType: string; ValueName: "version"; ValueData: "{#GetRevision()}"
