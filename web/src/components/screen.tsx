import { cn } from "@/lib/utils";
import type { PropsWithChildren } from "react";

export const Screen = ({
    children,
    className,
}: PropsWithChildren & { className?: string }) => {
    return (
        <main className={cn("flex h-screen w-full flex-1 p-4", className)}>
            {children}
        </main>
    );
};
