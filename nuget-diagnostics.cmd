@echo off
set "ProgramFiles=C:\Program Files"
set "ProgramFiles(x86)=C:\Program Files (x86)"
set "ProgramW6432=C:\Program Files"
set "CommonProgramFiles=C:\Program Files\Common Files"
set "CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files"
cd /d "%~dp0"
set "MSBUILDDISABLENODEREUSE=1"
dotnet restore CyberBank.PublisherGuard\CyberBank.PublisherGuard.csproj --configfile NuGet.Config --verbosity minimal > desktop-app-restore.log 2>&1
dotnet restore tests\CyberBank.PublisherGuard.Desktop.Tests\CyberBank.PublisherGuard.Desktop.Tests.csproj --configfile NuGet.Config --no-dependencies --verbosity minimal > desktop-tests-restore.log 2>&1
dotnet test tests\CyberBank.PublisherGuard.Desktop.Tests\CyberBank.PublisherGuard.Desktop.Tests.csproj --no-restore
dotnet test tests\CyberBank.PublisherGuard.Domain.Tests\CyberBank.PublisherGuard.Domain.Tests.csproj --no-restore
dotnet test tests\CyberBank.PublisherGuard.Application.Tests\CyberBank.PublisherGuard.Application.Tests.csproj --no-restore
dotnet test tests\CyberBank.PublisherGuard.Infrastructure.Tests\CyberBank.PublisherGuard.Infrastructure.Tests.csproj --no-restore
