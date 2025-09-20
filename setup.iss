
; Inno Setup Script for Vault2Env
; See https://jrsoftware.org/isinfo.php for documentation

#define MyAppName "Vault2Env"
#define MyAppVersion "0.0.3"
#define MyAppPublisher "Vault2Env"
#define MyAppURL "https://github.com/eagle/source/github/Vault2Env"
#define MyAppExeName "v2e.exe"
#define MyBuildPath "publish_output"
#define MySetupIcon "setup_icon.ico"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value for other applications.
AppId={{AUTO_GUID}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; The output installer file name
OutputBaseFilename=v2e-{#MyAppVersion}-setup
; The directory where the installer will be created
OutputDir=installer_output
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; We need admin privileges to write to Program Files and modify the system PATH
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; This is the main executable published from the .NET project
Source: "{#MyBuildPath}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; TODO: Add other files like README.md, LICENSE, etc. if desired
; Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Only create icons if the user wants them
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Code]
const
  ModPathName = 'ModPath';
  ModPathType = 'system';

procedure ModPathDir(Dir: string; Add: Boolean);
var
  OrigPath: string;
  NewPath: string;
  Delimiter: string;
  I: Integer;
  P: Integer;
  S: string;
begin
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', OrigPath)
  then begin
    // if the key doesn't exist, create it
    OrigPath := '';
  end;

  NewPath := '';
  Delimiter := ';';
  I := 1;
  repeat
    P := Pos(Delimiter, OrigPath);
    if P = 0 then
      S := OrigPath
    else
    begin
      S := Copy(OrigPath, 1, P - 1);
      OrigPath := Copy(OrigPath, P + 1, Length(OrigPath));
    end;
    if (CompareText(S, Dir) <> 0) and (S <> '') then
    begin
      if NewPath <> '' then
        NewPath := NewPath + Delimiter;
      NewPath := NewPath + S;
    end;
  until P = 0;

  if Add then
  begin
    if NewPath <> '' then
      NewPath := NewPath + Delimiter;
    NewPath := NewPath + Dir;
  end;

  RegWriteStringValue(HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', NewPath);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Add the installation directory to the PATH
    ModPathDir(ExpandConstant('{app}'), True);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    // Remove the installation directory from the PATH
    ModPathDir(ExpandConstant('{app}'), False);
  end;
end;
