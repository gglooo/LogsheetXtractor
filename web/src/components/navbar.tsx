import { ModeToggle } from "@/components/mode-toggle";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { baseLogsheetsPath } from "@/modules/logsheets/routes";
import { CreateTemplateAction } from "@/modules/templates/actions/create-template-action/create-template-action";
import { ListChecks, Menu, PlusIcon, Settings } from "lucide-react";
import { useIntl } from "react-intl";
import { Link } from "react-router-dom";

export function Navbar() {
    const intl = useIntl();

    return (
        <header className="sticky top-0 z-50 w-full border-b border-border bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
            <div className="flex h-14 items-center justify-between px-4 md:px-8">
                <div className="flex items-center gap-4">
                    <Link to="/" className="flex items-center space-x-2">
                        <span className="font-bold text-lg">
                            {intl.formatMessage({
                                id: "navbar.title",
                                defaultMessage: "LogsheetXtractor",
                            })}
                        </span>
                    </Link>
                </div>

                <div className="hidden md:flex items-center gap-4">
                    <Button variant="outline" asChild>
                        <Link to={`${baseLogsheetsPath}/gamified-proofread`}>
                            <ListChecks className="mr-2 h-4 w-4" />
                            {intl.formatMessage({
                                id: "navbar.proofreading.quick",
                                defaultMessage: "Quick review",
                            })}
                        </Link>
                    </Button>
                    <CreateTemplateAction />
                    <Button variant="outline" size="icon" asChild>
                        <Link
                            to="/settings"
                            title={intl.formatMessage({
                                id: "navbar.settings",
                                defaultMessage: "Settings",
                            })}
                        >
                            <Settings className="h-5 w-5" />
                        </Link>
                    </Button>
                    <ModeToggle />
                </div>

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
                            <DropdownMenuItem asChild>
                                <Link
                                    to={`${baseLogsheetsPath}/gamified-proofread`}
                                    className="w-full cursor-pointer"
                                >
                                    <ListChecks className="mr-2 h-4 w-4" />
                                    {intl.formatMessage({
                                        id: "navbar.proofreading.quick",
                                        defaultMessage: "Quick review",
                                    })}
                                </Link>
                            </DropdownMenuItem>
                            <DropdownMenuItem asChild>
                                <Link
                                    to="/settings"
                                    className="w-full cursor-pointer"
                                >
                                    <Settings className="mr-2 h-4 w-4" />
                                    {intl.formatMessage({
                                        id: "navbar.settings",
                                        defaultMessage: "Settings",
                                    })}
                                </Link>
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </div>
            </div>
        </header>
    );
}
