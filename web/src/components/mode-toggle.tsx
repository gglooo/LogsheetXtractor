import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Moon, Sun } from "lucide-react";
import { useTheme } from "next-themes";
import { useIntl } from "react-intl";

export function ModeToggle() {
    const { setTheme } = useTheme();
    const intl = useIntl();

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="outline" size="icon">
                    <Sun className="h-[1.2rem] w-[1.2rem] rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
                    <Moon className="absolute h-[1.2rem] w-[1.2rem] rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
                    <span className="sr-only">
                        {intl.formatMessage({
                            id: "theme.toggle",
                            defaultMessage: "Toggle theme",
                        })}
                    </span>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => setTheme("light")}>
                    {intl.formatMessage({
                        id: "theme.light",
                        defaultMessage: "Light",
                    })}
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setTheme("dark")}>
                    {intl.formatMessage({
                        id: "theme.dark",
                        defaultMessage: "Dark",
                    })}
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setTheme("system")}>
                    {intl.formatMessage({
                        id: "theme.system",
                        defaultMessage: "System",
                    })}
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
