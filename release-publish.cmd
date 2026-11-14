@echo off
setlocal
set "ProgramFiles=C:\Program Files"
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "ProgramW6432=C:\Program Files"
set "CommonProgramFiles=C:\Program Files\Common Files"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
set "MSBUILDDISABLENODEREUSE=1"
cd /d "%~dp0"
dotnet restore CyberBank.PublisherGuard.Desktop\CyberBank.PublisherGuard.Desktop.csproj --configfile NuGet.Config --verbosity minimal
if errorlevel 1 exit /b 1
dotnet publish CyberBank.PublisherGuard.Desktop\CyberBank.PublisherGuard.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false -o artifacts\CyberBankPublisherGuard-win-x64
