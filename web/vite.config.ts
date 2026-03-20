/// <reference types="vitest/config" />

import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import path from "path";
import { defineConfig, loadEnv } from "vite";

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd());
    const port = env.VITE_PORT ?? 8080;

    return {
        plugins: [
            react({
                babel: {
                    plugins: [["babel-plugin-react-compiler"]],
                },
            }),
            tailwindcss(),
        ],
        resolve: {
            alias: {
                "@": path.resolve(__dirname, "./src"),
            },
        },
        server: {
            proxy: {
                "/api": {
                    target: `http://localhost:${port}`,
                    changeOrigin: true,
                    secure: false,
                },
                "/hubs": {
                    target: `http://localhost:${port}`,
                    changeOrigin: true,
                    secure: false,
                    ws: true,
                },
            },
        },
        test: {
            environment: "jsdom",
            globals: true,
            setupFiles: "./tests/setup/vitest.setup.ts",
            css: true,
            pool: "threads",
            clearMocks: true,
            restoreMocks: true,
            mockReset: true,
            include: ["tests/**/*.{test,spec}.{ts,tsx}"],
            exclude: ["tests/e2e/**", "node_modules/**", "dist/**"],
            coverage: {
                provider: "v8",
                reporter: ["text", "html", "lcov"],
                reportsDirectory: "./coverage",
                exclude: [
                    "src/i18n/**",
                    "src/assets/**",
                    "tests/**",
                    "**/*.d.ts",
                ],
            },
        },
    };
});
