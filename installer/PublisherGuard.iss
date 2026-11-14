#define AppName "CyberBank Publisher Guard"
#define AppVersion "0.2.0 Alpha"
#define AppPublisher "CyberBank Labs"
#define AppExeName "CyberBank.PublisherGuard.Desktop.exe"
#define ProductFolder "CyberBank\Publisher Guard"
#define BuildRoot "..\artifacts\CyberBankPublisherGuard-win-x64"
#define BrandIcon "..\CyberBank.PublisherGuard\Assets\Brand\publisher_guard_globe_l.ico"

[Setup]
AppId={{F6DA6889-447A-4B49-9F6C-849F89E4556A}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\{#ProductFolder}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=CyberBank-Publisher-Guard-Alpha-Setup-win-x64
SetupIconFile={#BrandIcon}
UninstallDisplayIcon={app}\{#AppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#BuildRoot}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
