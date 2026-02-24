import { execSync } from "child_process";
import fs from "fs";
import path from "path";

const i18nDir = path.join(process.cwd(), "src/i18n");

if (!fs.existsSync(i18nDir)) {
    fs.mkdirSync(i18nDir, { recursive: true });
}

console.log("Extracting translations to en.json...");
execSync(
    "npx formatjs extract 'src/**/*.{ts,tsx}' --out-file src/i18n/en.json --format simple",
    { stdio: "inherit" },
);

const enPath = path.join(i18nDir, "en.json");
const csPath = path.join(i18nDir, "cs.json");
const dePath = path.join(i18nDir, "de.json");

const enData = JSON.parse(fs.readFileSync(enPath, "utf-8"));

[csPath, dePath].forEach((filePath) => {
    let existingData = {};
    if (fs.existsSync(filePath)) {
        try {
            existingData = JSON.parse(fs.readFileSync(filePath, "utf-8"));
        } catch (e) {
            console.warn(`Failed to parse ${filePath}, starting fresh.`);
        }
    }

    const newData = {};
    for (const key of Object.keys(enData)) {
        newData[key] = existingData[key] || "";
    }

    fs.writeFileSync(filePath, JSON.stringify(newData, null, 4) + "\n");
    console.log(`Synced ${path.basename(filePath)}`);
});

console.log("Translation extraction and sync complete.");
