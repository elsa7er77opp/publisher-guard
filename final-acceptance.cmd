@echo off
setlocal
set "ProgramFiles=C:\Program Files"
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "ProgramW6432=C:\Program Files"
set "CommonProgramFiles=C:\Program Files\Common Files"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
set "MSBUILDDISABLENODEREUSE=1"
set "RESULT=0"
cd /d "%~dp0"

(
  echo === CyberBank Publisher Guard final acceptance ===
  echo.
  dotnet test tests\CyberBank.PublisherGuard.Domain.Tests\CyberBank.PublisherGuard.Domain.Tests.csproj --no-restore --logger "console;verbosity=normal"
  if errorlevel 1 set "RESULT=1"
  echo.
  dotnet test tests\CyberBank.PublisherGuard.Application.Tests\CyberBank.PublisherGuard.Application.Tests.csproj --no-restore --logger "console;verbosity=normal"
  if errorlevel 1 set "RESULT=1"
  echo.
  dotnet test tests\CyberBank.PublisherGuard.Infrastructure.Tests\CyberBank.PublisherGuard.Infrastructure.Tests.csproj --no-restore --logger "console;verbosity=normal"
  if errorlevel 1 set "RESULT=1"
  echo.
  dotnet test tests\CyberBank.PublisherGuard.Desktop.Tests\CyberBank.PublisherGuard.Desktop.Tests.csproj --no-restore --logger "console;verbosity=normal"
  if errorlevel 1 set "RESULT=1"
) > final-acceptance.log 2>&1

type final-acceptance.log
exit /b %RESULT%
