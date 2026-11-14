@echo off
set "ProgramFiles=C:\Program Files"
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "ProgramW6432=C:\Program Files"
set "CommonProgramFiles=C:\Program Files\Common Files"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
set "MSBUILDDISABLENODEREUSE=1"
cd /d "%~dp0"
dotnet restore CyberBank.PublisherGuard.Desktop\CyberBank.PublisherGuard.Desktop.csproj --configfile NuGet.Config --no-dependencies --verbosity minimal
if errorlevel 1 exit /b %errorlevel%
dotnet build CyberBank.PublisherGuard.Desktop\CyberBank.PublisherGuard.Desktop.csproj --no-restore
