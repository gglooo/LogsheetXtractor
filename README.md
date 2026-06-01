# LogsheetXtractor

This repository contains the full LogsheetXtractor application (backend + frontend + OCR/HTR integration).

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

For local, non-Docker development, see the component-specific guides:

- Backend: [`LogsheetXtractor.Solution/README.md`](LogsheetXtractor.Solution/README.md)
- Frontend: [`web/README.md`](web/README.md)

## Setup Instructions

### 1. Clone the Repository

Clone the repository.

```bash
git clone git@github.com:grp-bork/LogsheetXtractor.git
```

### 2. Set Up Credentials

The application requires API credentials for various services (Google Cloud, Azure, Amazon).
Create a `credentials` folder in the root directory and place your JSON credential files there.
Credentials can also be configured in the web application.

```bash
mkdir -p credentials
# Place your google-credentials.json, azure-credentials.json, etc. in this folder
```

**Directory Structure:**

```
.
├── credentials/
│ ├── google-credentials.json
│ ├── azure-credentials.json
│ └── amazon-credentials.json
├── docker-compose.yml
├── LICENSE
├── LogsheetXtractor.Solution
├── README.md
└── web
```

> **Note:** The `credentials` folder is mounted into the backend container at runtime.

### 3. Configuration

Docker Compose uses the Docker-specific backend configuration:

- `LogsheetXtractor.Solution/LogsheetXtractor.API/appsettings.Docker.json` (used for Docker environment)

Local backend settings are covered in the backend README.

## Run the Application

From the root directory, run:

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

### Run from published images (production)

CI publishes multi-arch images to GitHub Container Registry on every push to `main`:

- `ghcr.io/grp-bork/logsheetxtractor-backend:latest` (and `:commit-sha`)
- `ghcr.io/grp-bork/logsheetxtractor-frontend:latest` (and `:commit-sha`)

On a server with Docker, clone the repo (for `docker-compose.yml`, volumes, and config), then:

```bash
docker compose pull
docker compose up -d
```

Use the same `docker compose` command as locally; omit `--build` to run the pulled images. `docker compose up --build` still works for local development and tags built images with the names above.

## Testing

Detailed test setup lives in the component-specific READMEs. Common commands:

From `LogsheetXtractor.Solution/`:

```bash
dotnet test LogsheetXtractor.Solution.sln
```

From `web/`:

```bash
pnpm install
pnpm test
```

See the backend and frontend READMEs for prerequisites, coverage commands, E2E notes, and local development details.

### Docker Configuration Details

- **Backend Dockerfile**: Creates Python virtual environment and installs dependencies.
- **Frontend Dockerfile**: Builds and serves the web app.
- **appsettings.Docker.json**: Docker environment overrides.
- **docker-compose.yml**: Defines services, GHCR image names, and mounts local directories.

## Troubleshooting

- **Docker is not running:** Start Docker Desktop before running `docker compose up --build`.

- **Port conflicts:** The default ports are `3000` for the frontend, `8080` and `8081` for the backend, and `8978` for CloudBeaver. If one of these ports is already in use, change the published port in `docker-compose.yml`.

- **Container rebuild:** If dependencies or build artifacts are out of date:

```bash
docker compose up --build --force-recreate
```

- **Clean start:** If you need a fully clean run:

```bash
docker compose down -v
docker compose up --build
```

This **removes** Docker volumes. The bind-mounted `app_data/`, `credentials/`, and `cloudbeaver/` directories may still remain in the project directory.

- **Reset local application data:** Stop the containers, then remove or rename `app_data/`. This deletes the local SQLite database and uploaded files.

- **Missing OCR credentials:** Confirm that credential files exist under `credentials/` or upload credentials through the web UI. Docker mounts this directory into the backend container at `/app/credentials`.

- **OCR processing fails:** Check that the selected OCR provider credentials are valid and that the uploaded logsheet matches the selected template. Backend script execution depends on the installed `formhtr` package.
