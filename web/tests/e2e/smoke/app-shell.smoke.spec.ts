import { expect, test } from "@playwright/test";

test("app redirects root route to dashboard @smoke", async ({ page }) => {
    await page.goto("/");

    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(page.locator("body")).toBeVisible();
});

