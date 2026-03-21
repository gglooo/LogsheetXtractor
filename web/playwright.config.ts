import { defineConfig, devices } from "@playwright/test";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

const frontendPort = process.env.PLAYWRIGHT_PORT ?? "4173";
const backendPort = process.env.PLAYWRIGHT_BACKEND_PORT ?? "18080";
const localBaseURL = `http://127.0.0.1:${frontendPort}`;
const localBackendBaseURL = `http://127.0.0.1:${backendPort}`;
const externalBaseURL = process.env.PLAYWRIGHT_BASE_URL;
const useExternalBaseURL =
    typeof externalBaseURL === "string" && externalBaseURL.length > 0;
const baseURL = useExternalBaseURL ? externalBaseURL : localBaseURL;
const runId = process.env.PLAYWRIGHT_RUN_ID ?? `${Date.now()}`;
const repoRootPath = path.resolve(__dirname, "..");
const backendDockerfilePath = path.resolve(
    repoRootPath,
    "LogsheetXtractor.Solution/LogsheetXtractor.API/Dockerfile",
);
const backendImageTag =
    process.env.PLAYWRIGHT_BACKEND_IMAGE ?? "logsheetxtractor-api-e2e";
const backendContainerName = `logsheetxtractor-api-e2e-${runId}`;
const backendDataPath = path.join(
    os.tmpdir(),
    `logsheetxtractor-e2e-app-data-${runId}`,
);
const backendCredentialsPath = path.join(
    os.tmpdir(),
    `logsheetxtractor-e2e-credentials-${runId}`,
);
const processEnv = Object.fromEntries(
    Object.entries(process.env).filter(
        (entry): entry is [string, string] => typeof entry[1] === "string",
    ),
);

export default defineConfig({
    testDir: "./tests/e2e",
    globalTeardown: "./tests/e2e/global-teardown.ts",
    fullyParallel: true,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 2 : 0,
    workers: process.env.CI ? 1 : undefined,
    reporter: process.env.CI ? "github" : "list",
    use: {
        baseURL,
        trace: "on-first-retry",
    },
    webServer: useExternalBaseURL
        ? undefined
        : [
              {
                  command: `docker build -f "${backendDockerfilePath}" -t ${backendImageTag} "${repoRootPath}" && exec docker run --rm --name "${backendContainerName}" -p ${backendPort}:8080 -e ASPNETCORE_ENVIRONMENT=Docker -v "${backendDataPath}:/app/app_data" -v "${backendCredentialsPath}:/app/credentials" ${backendImageTag}`,
                  url: `${localBackendBaseURL}/api/templates`,
                  reuseExistingServer: false,
                  timeout: 600000,
                  env: {
                      ...processEnv,
                  },
              },
              {
                  command: `pnpm dev --host 127.0.0.1 --port ${frontendPort}`,
                  url: localBaseURL,
                  reuseExistingServer: !process.env.CI,
                  timeout: 120000,
                  env: {
                      ...processEnv,
                      VITE_PORT: backendPort,
                      VITE_DISABLE_SIGNALR: "true",
                  },
              },
          ],
    projects: [
        {
            name: "chromium",
            use: { ...devices["Desktop Chrome"] },
        },
    ],
});
