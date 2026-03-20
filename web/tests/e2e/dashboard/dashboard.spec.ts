import { expect, test } from "@playwright/test";
import {
    createTemplateViaDashboard,
    uniqueE2EName,
    useDefaultE2EClientState,
} from "../utils/e2e-flows";

test.beforeEach(async ({ page }) => {
    await useDefaultE2EClientState(page);
});

test("dashboard creates template via UI and opens logsheets list", async ({
    page,
}) => {
    await page.goto("/dashboard");

    const templateName = uniqueE2EName("E2E Template");
    await createTemplateViaDashboard(page, templateName);

    const templateCard = page
        .locator('[data-slot="card"]')
        .filter({ hasText: templateName })
        .first();
    await expect(templateCard).toBeVisible();

    await templateCard
        .getByRole("button", { name: /^Logsheets\s*\(\d+\)$/ })
        .click();

    await expect(page).toHaveURL(/\/templates\/[0-9a-fA-F-]+\/logsheets$/);
    await expect(page.getByText("No logsheets found.")).toBeVisible();
});

test("dashboard renders main shell with create template action", async ({
    page,
}) => {
    await page.goto("/dashboard");

    await expect(page.getByRole("button", { name: "Create template" })).toBeVisible();
});
