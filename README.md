# CyberBank Publisher Guard

Publisher Guard prepares a local delivery package from selected source files. Inventory and preflight code identify package issues. Operator decisions govern inclusion, and delivery evidence records the selected output. An acceptance test describes the workflow against a temporary fixture.

## Engineering scope

The installer declares Alpha 0.2.0. The checked-in domain project references missing package types. The broad Packages ignore rule also matches the expected domain source directory. A source-complete checkout is needed before claiming a reproducible build.

## Getting started

Use the .NET 10 SDK on Windows. From the repository root, build with `dotnet build CyberBank.PublisherGuard.slnx`. To run the isolated domain suite, use `dotnet test tests/CyberBank.PublisherGuard.Domain.Tests/CyberBank.PublisherGuard.Domain.Tests.csproj`. The current public checkout stops during compilation as described below.

## Status and verification

Alpha 0.2.0 packaging. Public checkout fails to build.

The domain test command failed at compilation (CS0234 and CS0246, including missing `PackageInclusionState`). No tests executed. The fixture documents intended behavior pending a successful run.

## Source and evidence

- [src](src)
- [RealFixtureDeliveryAcceptanceTests.cs](tests/CyberBank.PublisherGuard.Infrastructure.Tests/RealFixtureDeliveryAcceptanceTests.cs)
- [PublisherGuard.iss](installer/PublisherGuard.iss)

## Author

Ahmed Mohamed Elsayed Elreefy. Engineering work includes requirements, technical decisions, AI-assisted implementation, debugging and acceptance review.
