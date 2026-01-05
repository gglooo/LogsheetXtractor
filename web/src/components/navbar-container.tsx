import { cn } from "@/lib/utils";
import type { PropsWithChildren } from "react";

export const NavbarContainer = ({
    children,
    AsideContent,
    className,
}: PropsWithChildren<{
    className?: string;
    AsideContent?: React.ReactNode;
}>) => {
    return (
        <header
            className={cn(
                "sticky z-50 border-b border-border bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60 px-4 md:px-8",
                className
            )}
        >
            <div className="flex justify-between items-center">{children}</div>
            {AsideContent}
        </header>
    );
};
