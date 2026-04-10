# Backend Test Taxonomy Split Design (2026-04-02)

## Goal
Separate backend tests into explicit categories:
- Unit tests: isolated tests in `LogsheetXtractor.UnitTests`.
- Integration tests: component integration without HTTP in `LogsheetXtractor.IntegrationTests`.
- E2E tests: full-system tests through REST endpoints in `LogsheetXtractor.E2ETests`.

## Approved approach
Move-only split:
1. Move all current HTTP tests and HTTP test infrastructure (`WebApplicationFactory`, `HttpClient` endpoint tests) from `LogsheetXtractor.IntegrationTests` to `LogsheetXtractor.E2ETests`.
2. Move non-HTTP backend component tests from `LogsheetXtractor.UnitTests/Infrastructure/Services` into `LogsheetXtractor.IntegrationTests`.
3. Keep `LogsheetXtractor.UnitTests` for unit-style tests (mostly Application handler tests).

## Project wiring
- New project: `LogsheetXtractor.E2ETests` with xUnit + `Microsoft.AspNetCore.Mvc.Testing` and API project reference.
- `LogsheetXtractor.IntegrationTests` references Application/Infrastructure/Domain and test dependencies needed by moved service tests.
- Both projects retain xUnit runner config.

## Verification
Run:
1. `dotnet test LogsheetXtractor.Solution/LogsheetXtractor.UnitTests/LogsheetXtractor.UnitTests.csproj`
2. `dotnet test LogsheetXtractor.Solution/LogsheetXtractor.IntegrationTests/LogsheetXtractor.IntegrationTests.csproj`
3. `dotnet test LogsheetXtractor.Solution/LogsheetXtractor.E2ETests/LogsheetXtractor.E2ETests.csproj`
