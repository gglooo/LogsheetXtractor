# LogsheetXtractor

This repository contains the full LogsheetXtractor application (backend + frontend + OCR/HTR integration).

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

## Setup Instructions

### 1. Clone the Repository

Clone the repository.

```bash
git clone git@github.com:grp-bork/LogsheetXtractor.git
cd LogsheetXtractor
```

### 2. Set Up Credentials

The application requires API credentials for various services (Google Cloud, Azure, Amazon).
Create a `credentials` folder in the root of the `LogsheetXtractor` directory and place your JSON credential files there.
Credentials can also be configured in the web application.

```bash
mkdir -p credentials
# Place your google-credentials.json, azure-credentials.json, etc. in this folder
```

**Directory Structure:**

```
LogsheetXtractor/
├── credentials/
│   ├── google-credentials.json
│   ├── azure-credentials.json
│   └── amazon-credentials.json
├── LogsheetXtractor.Solution/
├── docker-compose.yml
└── ...
```

> **Note:** The `credentials` folder is mounted into the backend container at runtime.

### 3. Configuration (Optional)

You can adjust application settings (file paths, connection strings, credential paths) in:

- `LogsheetXtractor.Solution/LogsheetXtractor.API/appsettings.json`
- `LogsheetXtractor.Solution/LogsheetXtractor.API/appsettings.Docker.json` (used for Docker environment)

### 4. Local Backend Python Setup (Optional, for non-docker environment only)

Backend scripting uses the `formhtr` Python package. Use Python `>=3.10` (for example through `pyenv` or `conda`) and install API Python dependencies from:

```bash
cd LogsheetXtractor
python --version  # must be >= 3.10
python -m pip install -r LogsheetXtractor.Solution/LogsheetXtractor.API/requirements.txt
```

If needed, set `Python:InterpreterPath` in `appsettings.Development.json` to that environment's interpreter path.

## Run the Application

From `LogsheetXtractor/`, run:

```bash
docker compose up --build
```

This command will:

1. Build the .NET backend image.
2. Install required Python dependencies.
3. Build and start the React frontend container.
4. Apply database migrations and create storage directories (for example `app_data`).
5. Start all required services.

Access points:

- App (frontend): `http://localhost:3000`
- Backend API: `http://localhost:8080`
- CloudBeaver (DB UI): `http://localhost:8978`

### Docker Configuration Details

- **Backend Dockerfile**: Creates Python virtual environment and installs dependencies.
- **Frontend Dockerfile**: Builds and serves the web app.
- **appsettings.Docker.json**: Docker environment overrides.
- **docker-compose.yml**: Defines services and mounts local directories.

## Troubleshooting

- **Container rebuild:** If dependencies or build artifacts are out of date:

```bash
docker compose up --build --force-recreate
```

- **Clean start:** If you need a fully clean run and want to remove volumes (note: this will delete database data and storage files):

```bash
docker compose down -v
docker compose up --build
```
