import { expect, test } from "@playwright/test";
import { useDefaultE2EClientState } from "../utils/e2e-flows";

test.beforeEach(async ({ page }) => {
    await useDefaultE2EClientState(page);
});

test("settings saves and clears personal credentials against real backend", async ({
    page,
}) => {
    await page.goto("/settings");

    await expect(page.getByText("Missing credentials")).toBeVisible();

    await page.getByLabel("Google API key").fill("google-key-e2e");
    await page.getByLabel("Azure API key").fill("azure-key-e2e");
    await page.getByLabel("Amazon API key").fill("amazon-key-e2e");
    await page.getByRole("button", { name: "Save credentials" }).click();

    await expect(page.getByText("Using personal keys")).toBeVisible();

    await page.getByRole("button", { name: "Clear personal credentials" }).click();

    await expect(page.getByText("Missing credentials")).toBeVisible();
});

test("settings shows missing credentials when server defaults are absent", async ({
    page,
}) => {
    await page.goto("/settings");

    await expect(page.getByText("Missing credentials")).toBeVisible();
});
