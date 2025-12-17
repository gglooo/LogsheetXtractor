import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Menu, PlusIcon } from "lucide-react";
import { useIntl } from "react-intl";

export function Navbar() {
    const intl = useIntl();

    return (
        <header className="sticky top-0 z-50 w-full border-b border-border bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
            <div className="container flex h-14 max-w-screen-2xl items-center justify-between px-4 md:px-8">
                <div className="flex items-center gap-4">
                    <a href="/" className="flex items-center space-x-2">
                        <span className="font-bold text-lg">
                            {intl.formatMessage({
                                id: "navbar.logo",
                                defaultMessage: "LOGO",
                            })}
                        </span>
                    </a>
                </div>

                <div className="hidden md:flex items-center gap-2">
                    <Button variant="outline" size="sm">
                        {intl.formatMessage({
                            id: "navbar.login",
                            defaultMessage: "Log in",
                        })}
                    </Button>
                    <Button size="sm">
                        <PlusIcon />
                        {intl.formatMessage({
                            id: "navbar.newTemplate",
                            defaultMessage: "New template",
                        })}
                    </Button>
                </div>

                {/* Mobile Section */}
                <div className="md:hidden">
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button
                                variant="outline"
                                size="icon"
                                className="h-9 w-9"
                            >
                                <Menu className="h-5 w-5" />
                                <span className="sr-only">
                                    {intl.formatMessage({
                                        id: "navbar.toggleMenu",
                                        defaultMessage: "Toggle menu",
                                    })}
                                </span>
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end" className="w-50">
                            <DropdownMenuLabel>
                                {intl.formatMessage({
                                    id: "navbar.menu",
                                    defaultMessage: "Menu",
                                })}
                            </DropdownMenuLabel>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem>
                                {intl.formatMessage({
                                    id: "navbar.login",
                                    defaultMessage: "Log in",
                                })}
                            </DropdownMenuItem>
                            <DropdownMenuItem>
                                <PlusIcon className="mr-2 h-4 w-4" />
                                {intl.formatMessage({
                                    id: "navbar.newTemplate",
                                    defaultMessage: "New template",
                                })}
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </div>
            </div>
        </header>
    );
}
