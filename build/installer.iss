#define AppName "TuitionDesk"
#define AppVersion "0.1.0"
#define AppPublisher "TuitionDesk"
#define AppExeName "TuitionDesk.exe"

[Setup]
AppId={{7D5B2A8E-6F4C-4B1E-9C3A-1E5D8A2B7C90}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\TuitionDesk
DefaultGroupName=TuitionDesk
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\artifacts
OutputBaseFilename=TuitionDesk-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
AppMutex=Local\TuitionDesk
UninstallDisplayIcon={app}\{#AppExeName}
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\artifacts\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
const
  WebView2ClientKey = 'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';

function WebView2Version(): String;
var
  Version: String;
begin
  Result := '';
  if RegQueryStringValue(HKCU, WebView2ClientKey, 'pv', Version) then
  begin
    Result := Version;
    Exit;
  end;
  if RegQueryStringValue(HKLM64, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) then
  begin
    Result := Version;
    Exit;
  end;
  if RegQueryStringValue(HKLM64, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) then
  begin
    Result := Version;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if WebView2Version() = '' then
  begin
    SuppressibleMsgBox(
      'TuitionDesk needs the Microsoft Edge WebView2 Runtime.' + #13#10 +
      'Install it from https://go.microsoft.com/fwlink/p/?LinkId=2124703 before launching the app.',
      mbInformation, MB_OK, IDOK);
  end;
end;
