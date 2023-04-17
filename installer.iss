#define BASE_DIR         SourcePath + 'bin\Release\net6.0\win10-x64'
#define SOURCE_DIR       BASE_DIR + '\publish'
#define INSTALLER_DIR    BASE_DIR + '\installer'

#define AppVer           GetVersionNumbersString(SOURCE_DIR + "\Zephyr.exe")

[Setup]
AppName="Zephyr"
AppVersion="{#AppVer}"
AppVerName="Zephyr V{#AppVer}"
VersionInfoVersion="{#AppVer}"

ArchitecturesInstallIn64BitMode=x64 

AppMutex="ZEPHYR"
AppId="ZEPHYR"
SourceDir="{#BASE_DIR}"
DefaultDirName="{commonpf}\Zephyr"
DefaultGroupName="Zephyr"

OutPutDir="{#INSTALLER_DIR}"
OutputBaseFilename="Zephyr_{#AppVer}_Installer"
SetupIconFile="{#SourcePath}\icon.ico"
SetupMutex="ZEPHYR"
UninstallDisplayIcon="{app}\Zephyr.exe"

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs
Source: "Microsoft.Playwright.dll"; DestDir: "{app}"

[Icons]
Name: "{group}\Zephyr"; Filename: "{app}\Zephyr.exe";  WorkingDir: "{app}"
Name: "{userstartmenu}\Zephyr"; Filename: "{app}\Zephyr.exe";  WorkingDir: "{app}"

[Run]
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\playwright.ps1"" install chromium"; WorkingDir: {app}; \
  Flags: runhidden; Description: "Playwright install"; StatusMsg: "Installing playwright";