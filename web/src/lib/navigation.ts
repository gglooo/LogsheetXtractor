export type RouteParentEntry = {
    pattern: RegExp;
    parent: string;
};

export const routeParentMap: RouteParentEntry[] = [
    {
        // /template-editor/:id  →  /dashboard
        pattern: /^\/template-editor\/[^/]+$/,
        parent: "/dashboard",
    },
    {
        // /templates/:templateId/logsheets/upload  →  /templates/:templateId/logsheets
        pattern: /^\/templates\/([^/]+)\/logsheets\/upload/,
        parent: "/templates/{0}/logsheets",
    },
    {
        // /templates/:templateId/logsheets/:id/align|proofread  →  /templates/:templateId/logsheets
        pattern: /^\/templates\/([^/]+)\/logsheets\/[^/]+\/(align|proofread)$/,
        parent: "/templates/{0}/logsheets",
    },
    {
        // /templates/:templateId/logsheets  →  /dashboard
        pattern: /^\/templates\/[^/]+\/logsheets$/,
        parent: "/dashboard",
    },
    {
        // /logsheets/gamified-proofread  →  /dashboard
        pattern: /^\/logsheets\/gamified-proofread$/,
        parent: "/dashboard",
    },
    {
        // /settings  →  /dashboard
        pattern: /^\/settings$/,
        parent: "/dashboard",
    },
];

export const resolveParentPath = (pathname: string): string | undefined => {
    for (const entry of routeParentMap) {
        const match = pathname.match(entry.pattern);
        if (match) {
            return entry.parent.replace(/\{(\d+)\}/g, (_, index: string) => {
                return match[parseInt(index) + 1] ?? "";
            });
        }
    }
    return undefined;
};
