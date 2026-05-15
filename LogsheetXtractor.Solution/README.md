# LogsheetXtractor Backend

.NET backend for LogsheetXtractor. It exposes the HTTP API, coordinates application workflows, persists data in SQLite, runs OCR/HTR integration through the installed `formhtr` Python package, and publishes processing updates through SignalR.

## Prerequisites

- .NET SDK `10.0.100` or newer compatible SDK. The solution has `global.json` with `rollForward` set to `latestMajor`.
- Python `>=3.10` for local OCR/HTR script execution.
- Access to OCR provider credentials if you want to process logsheets end to end.
- Optional: `reportgenerator` for local coverage reports.
- Optional: `dotnet-ef` for creating migrations.

Docker users normally do not need the local Python setup because the backend image installs `LogsheetXtractor.API/requirements.txt`.

## Setup

From `logsheetXtractor/LogsheetXtractor.Solution/`:

```bash
dotnet restore LogsheetXtractor.Solution.sln
```

The formHTR tool has a specific installation procedure. It requires some system dependencies that must be installed separately. Refer to the [formHTR package documentation](https://pypi.org/project/formhtr/) for detailed instructions.

After following the installation instructions, install Python dependencies used by the backend script adapter:

```bash
python --version
python -m pip install -r LogsheetXtractor.API/requirements.txt
```

The requirements file currently installs `formhtr`. Runtime integration expects the CLI contract exposed by:

```bash
python -m formhtr.cli
```

## Configuration

Local development uses `LogsheetXtractor.API/appsettings.Development.json`.

Important settings:

- `ConnectionStrings:DefaultConnection`: SQLite database path. The default is `../../app_data/FormHTR.db`.
- `Storage:LocalStoragePath`: uploaded/generated file storage path. The default is `../../app_data/storage`.
- `Python:InterpreterPath`: Python interpreter used for OCR/HTR script execution. Change this to the interpreter where `formhtr` is installed.
- `Credentials:*ApiKeyPath`: default file paths for mounted provider credentials.
- `Credentials:CookieSecure`: set to `false` for local HTTP development.

The API creates the storage directory and applies pending EF Core migrations at startup.

Relative paths are resolved by the running backend process. If you run from a different working directory than your IDE uses, confirm these paths still resolve to `logsheetXtractor/app_data`.

## Credentials

For file-based credentials, create `logsheetXtractor/credentials/`:

```bash
cd ..
mkdir -p credentials
```

Expected default files:

- `credentials/google-credentials.json`
- `credentials/azure-credentials.json`
- `credentials/amazon-credentials.json`

Credentials can also be provided through the web UI. User-provided credentials are tracked through cookie-backed credential context and propagated into background Wolverine flows.

## Local Development

Run the API from `logsheetXtractor/LogsheetXtractor.Solution/`:

```bash
dotnet run --project LogsheetXtractor.API
```

The `http` launch profile serves the API at:

- `http://localhost:5226`

The `https` profile serves:

- `https://localhost:7237`
- `http://localhost:5226`

Swagger UI is enabled in the `Development` environment.

For the full app through Docker Compose, use the root README instructions instead.

## Project Structure

- `LogsheetXtractor.API`: Wolverine HTTP endpoints, startup, SignalR hub, exception handling, HTTP result mapping.
- `LogsheetXtractor.Application`: commands, queries, handlers, DTOs, interfaces, validation, application errors.
- `LogsheetXtractor.Infrastructure`: EF Core persistence, migrations, storage, OCR credentials, PDF/QR/script service implementations.
- `LogsheetXtractor.Domain`: entities, enums, and value objects.
- `LogsheetXtractor.UnitTests`: handler and focused unit tests.
- `LogsheetXtractor.IntegrationTests`: infrastructure and persistence integration tests.
- `LogsheetXtractor.ArchitectureTests`: layer dependency rules.
- `LogsheetXtractor.E2ETests`: API and business-flow integration tests.

## Backend Flow

The intended business-operation path is:

```text
Wolverine HTTP endpoint -> Application handler -> IAppDbContext checks/orchestration -> Infrastructure service calls -> Result return -> HTTP mapping
```

## Testing

Run all backend tests:

```bash
dotnet test LogsheetXtractor.Solution.sln
```

Run a specific project:

```bash
dotnet test LogsheetXtractor.UnitTests/LogsheetXtractor.UnitTests.csproj
dotnet test LogsheetXtractor.IntegrationTests/LogsheetXtractor.IntegrationTests.csproj
dotnet test LogsheetXtractor.ArchitectureTests/LogsheetXtractor.ArchitectureTests.csproj
dotnet test LogsheetXtractor.E2ETests/LogsheetXtractor.E2ETests.csproj
```

Generate coverage data:

```bash
dotnet test LogsheetXtractor.Solution.sln --collect:"XPlat Code Coverage" --results-directory TestResults/Coverage
```

Generate a readable report if `reportgenerator` is installed:

```bash
reportgenerator '-reports:TestResults/Coverage/*/coverage.cobertura.xml' '-targetdir:TestResults/CoverageReport' '-reporttypes:TextSummary;Html'
```

## Migrations

EF Core migrations live under:

```text
LogsheetXtractor.Infrastructure/Persistence/Migrations
```

The application applies pending migrations on startup. When changing the persistence model, create migrations against the infrastructure project and API startup project:

```bash
dotnet ef migrations add <MigrationName> --project LogsheetXtractor.Infrastructure --startup-project LogsheetXtractor.API --output-dir Persistence/Migrations
```

Install the EF tool first if the command is not available:

```bash
dotnet tool install --global dotnet-ef
```

## Troubleshooting

- **Python execution fails:** confirm `Python:InterpreterPath` points to an environment with `formhtr` installed, then run `python -m formhtr.cli`.
- **OCR processing fails:** confirm the selected provider credentials are valid and that the uploaded logsheet matches the selected template.
- **SQLite or storage path errors:** confirm `../../app_data` is writable from the API working directory, or override the paths in `appsettings.Development.json`.
- **Port conflict:** change the `applicationUrl` in `LogsheetXtractor.API/Properties/launchSettings.json` or pass a different `ASPNETCORE_URLS` value.
