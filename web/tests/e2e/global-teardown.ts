import { execSync } from "node:child_process";

const removeContainersByNamePrefix = (namePrefix: string) => {
    try {
        const containerIds = execSync(
            `docker ps -aq --filter "name=${namePrefix}"`,
            { encoding: "utf8" },
        )
            .trim()
            .split(/\s+/)
            .filter(Boolean);

        if (containerIds.length > 0) {
            execSync(`docker rm -f ${containerIds.join(" ")}`, {
                stdio: "ignore",
            });
        }
    } catch {
        // Ignore teardown cleanup failures.
    }
};

async function globalTeardown() {
    removeContainersByNamePrefix("logsheetxtractor-api-e2e");
    removeContainersByNamePrefix("logsheetxtractor-e2e-backend");
}

export default globalTeardown;
