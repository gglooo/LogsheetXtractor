import { expect, type Page } from "@playwright/test";

const minimalPdfBuffer = Buffer.from(
    "%PDF-1.1\n1 0 obj<</Type/Catalog>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF\n",
);

const importedTemplateConfigBuffer = Buffer.from(
    JSON.stringify({
        content: [],
        to_ignore: [],
        width: 1200,
        height: 1700,
    }),
);

export const useDefaultE2EClientState = async (page: Page) => {
    await page.addInitScript(() => {
        window.localStorage.setItem("app-locale", "en");
        window.localStorage.setItem(
            "user-settings",
            JSON.stringify({
                uglyCheckboxes: false,
                automaticAlignmentOnUpload: false,
            }),
        );
    });
};

export const uniqueE2EName = (prefix: string) =>
    `${prefix}-${Date.now()}-${Math.random().toString(16).slice(2, 8)}`;

export const createTemplateViaDashboard = async (
    page: Page,
    templateName: string,
) => {
    await page.getByRole("button", { name: "Create template" }).click();

    const dialog = page.getByRole("dialog");
    await expect(dialog).toBeVisible();

    const fileInputs = dialog.locator('input[type="file"]');
    await fileInputs.nth(0).setInputFiles({
        name: "template.pdf",
        mimeType: "application/pdf",
        buffer: minimalPdfBuffer,
    });
    await fileInputs.nth(1).setInputFiles({
        name: "template-config.json",
        mimeType: "application/json",
        buffer: importedTemplateConfigBuffer,
    });

    await dialog.getByLabel("Template name").fill(templateName);
    await dialog
        .getByRole("button", { name: "Create template", exact: true })
        .click();

    await expect(dialog).toBeHidden();
};

export const uploadLogsheetViaUi = async (
    page: Page,
    fileName = "sheet-001.pdf",
) => {
    await page.locator('input[type="file"]').first().setInputFiles({
        name: fileName,
        mimeType: "application/pdf",
        buffer: minimalPdfBuffer,
    });

    await page.getByRole("button", { name: "Upload", exact: true }).click();
};
