# CyberBank Publisher Guard

Publisher Guard prepares a local delivery package from selected source files. Inventory and preflight code identify package issues. Operator decisions govern inclusion, and delivery evidence records the selected output. An acceptance test describes the workflow against a temporary fixture.

## Engineering scope

The installer declares Alpha 0.2.0. The review branch restores the original inventory, inclusion and delivery implementations from the development checkout. The root NuGet cache remains ignored.

## Getting started

Use the .NET 10 SDK on Windows. From the repository root, build with `dotnet build CyberBank.PublisherGuard.slnx`. To run the isolated domain suite, use `dotnet test tests/CyberBank.PublisherGuard.Domain.Tests/CyberBank.PublisherGuard.Domain.Tests.csproj`. Run all four suites with `dotnet test CyberBank.PublisherGuard.slnx`.

## Status and verification

Alpha 0.2.0 packaging.

The restored solution passed domain, application, infrastructure and desktop tests on Windows with the .NET 10 SDK. This check did not install the packaged product.

## Source and evidence

- [src](src)
- [RealFixtureDeliveryAcceptanceTests.cs](tests/CyberBank.PublisherGuard.Infrastructure.Tests/RealFixtureDeliveryAcceptanceTests.cs)
- [PublisherGuard.iss](installer/PublisherGuard.iss)

## Author

Ahmed Mohamed Elsayed Elreefy. Engineering work includes requirements, technical decisions, AI-assisted implementation, debugging and acceptance review.
