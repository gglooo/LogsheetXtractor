# WebFormHTR Backend

This repository contains the backend for the WebFormHTR application, built with .NET and Python.

## Prerequisites

-   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
-   [Git](https://git-scm.com/)

## Setup Instructions

### 1. Clone the Repository

Clone the repository with submodules to ensure all dependencies are downloaded.

```bash
git clone --recursive https://github.com/gglooo/dipr
cd dipr/webFormHTR
```

### 2. Setup Credentials

The application requires API credentials for various services (Google Cloud, Azure, Amazon).
Create a `credentials` folder in the root of the `webFormHTR` directory and place your JSON credential files there.

```bash
mkdir -p credentials
# Place your google-credentials.json, azure-credentials.json, etc. in this folder
```

**Directory Structure:**

```
webFormHTR/
├── credentials/
│   ├── google-credentials.json
│   ├── azure-credentials.json
│   └── amazon-credentials.json
├── formHTR/
├── WebFormHTR.Solution/
├── docker-compose.yml
└── ...
```

> **Note:** The `credentials` folder is mounted into the Docker container at runtime.

### 3. Configuration

You can adjust application settings, including file paths, connection strings, and credential paths, in the `appsettings.json` (or `appsettings.Docker.json` for Docker environment) located in `WebFormHTR.Solution/WebFormHTR.API/`.

## Running the Application

To run the backend using Docker Compose:

```bash
docker compose up --build
```

This command will:

1.  Build the .NET backend image.
2.  Install all required Python dependencies.
3.  Start the service on ports `8080` (HTTP) and `8081` (HTTPS).
4.  Automatically apply database migrations and create necessary storage directories (e.g., `app_data`).

### Docker Configuration details

-   **Dockerfile**: Configured to create a Python virtual environment and install dependencies.
-   **appsettings.Docker.json**: Overrides for the Docker environment.
-   **docker-compose.yml**: Mounts local directories.

## Troubleshooting

-   **Python Dependencies**: If you see errors related to missing Python modules, rebuild the container:
    ```bash
    docker compose up --build --force-recreate
    ```

## Running the Frontend

The frontend is a React application located in the `web` directory.

### 1. Prerequisites

-   [Node.js](https://nodejs.org/) (I use v22.20.0)
-   [pnpm](https://pnpm.io/)

### 2. Setup and Run

Navigate to the `web` directory, install dependencies, and start the development server.

```bash
cd web
pnpm install
pnpm dev
```

The frontend will run on `http://localhost:5173`.
