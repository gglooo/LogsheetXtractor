import { I18nProvider } from "@/components/i18n-provider";
import { ThemeProvider } from "@/components/theme-provider";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, type RenderOptions } from "@testing-library/react";
import type { ReactElement, ReactNode } from "react";
import { MemoryRouter } from "react-router-dom";
import { createTestQueryClient } from "./query-client";

type RenderWithProvidersOptions = Omit<RenderOptions, "wrapper"> & {
    route?: string;
    queryClient?: QueryClient;
};

export const renderWithProviders = (
    ui: ReactElement,
    options?: RenderWithProvidersOptions,
) => {
    const {
        route = "/",
        queryClient = createTestQueryClient(),
        ...renderOptions
    } = options ?? {};

    const Wrapper = ({ children }: { children: ReactNode }) => (
        <QueryClientProvider client={queryClient}>
            <I18nProvider>
                <ThemeProvider
                    defaultTheme="system"
                    storageKey="vite-ui-theme"
                    attribute="class"
                >
                    <MemoryRouter initialEntries={[route]}>
                        {children}
                    </MemoryRouter>
                </ThemeProvider>
            </I18nProvider>
        </QueryClientProvider>
    );

    return render(ui, { wrapper: Wrapper, ...renderOptions });
};
