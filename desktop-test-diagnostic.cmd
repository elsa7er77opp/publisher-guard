@echo off
set "ProgramFiles=C:\Program Files"
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "ProgramW6432=C:\Program Files"
set "CommonProgramFiles=C:\Program Files\Common Files"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
set "MSBUILDDISABLENODEREUSE=1"
cd /d "%~dp0"
dotnet test tests\CyberBank.PublisherGuard.Desktop.Tests\CyberBank.PublisherGuard.Desktop.Tests.csproj --no-restore --logger "console;verbosity=normal" > desktop-test.log 2>&1
