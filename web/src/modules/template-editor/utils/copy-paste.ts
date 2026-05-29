export const copy = async <T>(items: T[]) => {
    try {
        const itemString = JSON.stringify(items);
        await navigator.clipboard.writeText(itemString);
    } catch (e) {
        console.error("Failed to copy items to clipboard:", e);
    }
};

export const paste = async <T>(
    onError?: (e: unknown) => void
): Promise<T[] | null> => {
    try {
        const itemString = await navigator.clipboard.readText();
        return JSON.parse(itemString) as T[];
    } catch (e) {
        onError?.(e);
        return null;
    }
};
