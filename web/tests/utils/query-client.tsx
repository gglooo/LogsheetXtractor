import {
    QueryClient,
    QueryClientProvider,
    type QueryClientConfig,
} from "@tanstack/react-query";
import type { ReactNode } from "react";

export const createTestQueryClient = (
    config?: QueryClientConfig,
): QueryClient => {
    return new QueryClient({
        defaultOptions: {
            queries: {
                retry: false,
                gcTime: 0,
            },
            mutations: {
                retry: false,
            },
        },
        ...config,
    });
};

export const createQueryClientWrapper = (queryClient: QueryClient) => {
    return ({ children }: { children: ReactNode }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
};

