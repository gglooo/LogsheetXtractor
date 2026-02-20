import { NavbarContainer } from "@/components/navbar-container";
import { Button } from "@/components/ui/button";
import { ArrowDownUp, ArrowLeft, Shuffle } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";

type GamifiedMode = "random" | "sequential";

type GamifiedNavbarProps = {
    mode: GamifiedMode;
    onModeChange: (mode: GamifiedMode) => void;
    verifiedCount: number;
};

export const GamifiedNavbar = ({
    mode,
    onModeChange,
    verifiedCount,
}: GamifiedNavbarProps) => {
    const intl = useIntl();
    const navigate = useNavigate();

    return (
        <NavbarContainer>
            <div className="flex flex-col sm:flex-row w-full justify-between items-start sm:items-center py-3 sm:py-4 gap-3 sm:gap-4">
                <div className="flex items-center gap-3 sm:gap-4">
                    <ArrowLeft
                        className="cursor-pointer shrink-0"
                        onClick={() => navigate(-1)}
                    />
                    <span className="text-base sm:text-lg font-bold truncate">
                        {intl.formatMessage({
                            id: "gamified.navbar.title",
                            defaultMessage: "Gamified proofreading",
                        })}
                    </span>
                </div>
                <div className="flex items-center gap-3 self-end sm:self-auto">
                    {verifiedCount > 0 && (
                        <span className="text-xs sm:text-sm text-muted-foreground whitespace-nowrap">
                            {intl.formatMessage(
                                {
                                    id: "gamified.navbar.verified",
                                    defaultMessage:
                                        "{count} verified this session",
                                },
                                { count: verifiedCount },
                            )}
                        </span>
                    )}
                    <div className="flex items-center rounded-md border border-border overflow-hidden shrink-0">
                        <Button
                            variant={mode === "random" ? "default" : "ghost"}
                            size="sm"
                            className="rounded-none gap-1.5 h-8 px-2 sm:px-3 text-xs sm:text-sm"
                            onClick={() => onModeChange("random")}
                        >
                            <Shuffle className="h-3.5 w-3.5" />
                            {intl.formatMessage({
                                id: "gamified.mode.random",
                                defaultMessage: "Random",
                            })}
                        </Button>
                        <Button
                            variant={
                                mode === "sequential" ? "default" : "ghost"
                            }
                            size="sm"
                            className="rounded-none gap-1.5 h-8 px-2 sm:px-3 text-xs sm:text-sm"
                            onClick={() => onModeChange("sequential")}
                        >
                            <ArrowDownUp className="h-3.5 w-3.5" />
                            {intl.formatMessage({
                                id: "gamified.mode.sequential",
                                defaultMessage: "Sequential",
                            })}
                        </Button>
                    </div>
                </div>
            </div>
        </NavbarContainer>
    );
};
