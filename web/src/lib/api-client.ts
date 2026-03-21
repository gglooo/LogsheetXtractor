export class ApiValidationError extends Error {
    public errorMessages: string[];

    constructor(message: string, errorMessages: string[]) {
        super(message);
        this.name = "ApiValidationError";
        this.errorMessages = errorMessages;
    }
}

export function setupGlobalFetchInterceptor() {
    const originalFetch = window.fetch;

    window.fetch = async (...args) => {
        const response = await originalFetch(...args);

        if (!response.ok) {
            try {
                const clonedResponse = response.clone();
                const data = await clonedResponse.json();

                if (
                    data &&
                    data.code === "VALIDATION_ERROR" &&
                    Array.isArray(data.errorMessages)
                ) {
                    throw new ApiValidationError(
                        data.detail ?? "Validation Error",
                        data.errorMessages,
                    );
                }
            } catch (e) {
                if (e instanceof ApiValidationError) {
                    throw e;
                }
            }
        }

        return response;
    };
}
