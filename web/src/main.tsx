import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { pdfjs } from "react-pdf";
import App from "./App.tsx";
import "./index.css";
// standard import for vite
import pdfWorker from "pdfjs-dist/build/pdf.worker.min.mjs?url";

pdfjs.GlobalWorkerOptions.workerSrc = pdfWorker;

import {
    ApiValidationError,
    setupGlobalFetchInterceptor,
} from "@/lib/api-client";
import {
    MutationCache,
    QueryCache,
    QueryClient,
    QueryClientProvider,
} from "@tanstack/react-query";
import { BrowserRouter } from "react-router-dom";
import { toast } from "sonner";
import { I18nProvider } from "./components/i18n-provider";
import { ThemeProvider } from "./components/theme-provider";

setupGlobalFetchInterceptor();

const handleApiError = (error: Error) => {
    if (error instanceof ApiValidationError) {
        error.errorMessages.forEach((msg) => {
            toast.error(msg, {
                duration: 5000,
            });
        });
    }
};

const queryClient = new QueryClient({
    mutationCache: new MutationCache({
        onError: handleApiError,
    }),
    queryCache: new QueryCache({
        onError: handleApiError,
    }),
});

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <QueryClientProvider client={queryClient}>
            <I18nProvider>
                <ThemeProvider
                    defaultTheme="system"
                    storageKey="vite-ui-theme"
                    attribute="class"
                >
                    <BrowserRouter>
                        <App />
                    </BrowserRouter>
                </ThemeProvider>
            </I18nProvider>
        </QueryClientProvider>
    </StrictMode>,
);
