export const getDuplicates = <T>(arr: T[]) => {
    const seen = new Set<T>();
    const duplicates = new Set<T>();

    for (const item of arr) {
        if (seen.has(item)) {
            duplicates.add(item);
        } else {
            seen.add(item);
        }
    }

    return duplicates;
};
