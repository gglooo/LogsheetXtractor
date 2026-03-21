import { expect, test } from "@playwright/test";
import {
    createTemplateViaDashboard,
    uniqueE2EName,
    uploadLogsheetViaUi,
    useDefaultE2EClientState,
} from "../utils/e2e-flows";

const missingTemplateId = "aaaaaaaa-1111-4111-8111-111111111111";

test.beforeEach(async ({ page }) => {
    await useDefaultE2EClientState(page);
});

test("logsheets table shows uploaded row from backend and allows selection", async ({
    page,
}) => {
    await page.goto("/dashboard");

    const templateName = uniqueE2EName("E2E Logsheets Template");
    await createTemplateViaDashboard(page, templateName);

    const templateCard = page
        .locator('[data-slot="card"]')
        .filter({ hasText: templateName })
        .first();
    await expect(templateCard).toBeVisible();

    await templateCard.getByRole("button", { name: /Add logsheets/i }).click();
    await expect(page).toHaveURL(/\/templates\/[0-9a-fA-F-]+\/logsheets\/upload$/);

    await uploadLogsheetViaUi(page, "sheet-001.pdf");

    await expect(page).toHaveURL(/\/templates\/[0-9a-fA-F-]+\/logsheets$/);
    await expect(page.getByText("sheet-001.pdf")).toBeVisible();

    await page.getByText("sheet-001.pdf").click();

    await expect(page.getByText("1 logsheet(s) selected")).toBeVisible();
});

test("logsheets table shows empty state for template without logsheets", async ({
    page,
}) => {
    await page.goto(`/templates/${missingTemplateId}/logsheets`);

    await expect(page.getByText("No logsheets found.")).toBeVisible();
});
