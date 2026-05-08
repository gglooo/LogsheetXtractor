import { GamifiedNavbar } from "@/modules/logsheets/proofreading/components/gamified-navbar";
import { RandomModeView } from "@/modules/logsheets/proofreading/components/random-mode-view";
import { SequentialModeView } from "@/modules/logsheets/proofreading/components/sequential-mode-view";
import { useCallback, useState } from "react";

type GamifiedMode = "random" | "sequential";

const STORAGE_KEY = "gamified-proofreading-mode";

const loadSavedMode = (): GamifiedMode => {
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved === "sequential" ? "sequential" : "random";
};

export const GamifiedProofreadingPage = () => {
    const [mode, setMode] = useState<GamifiedMode>(loadSavedMode);
    const [verifiedCount, setVerifiedCount] = useState(0);

    const handleModeChange = (newMode: GamifiedMode) => {
        localStorage.setItem(STORAGE_KEY, newMode);
        setMode(newMode);
    };

    const handleVerifiedCountChange = useCallback((delta: number) => {
        setVerifiedCount((prev) => prev + delta);
    }, []);

    return (
        <div className="flex flex-col h-screen overflow-hidden bg-background">
            <GamifiedNavbar
                mode={mode}
                onModeChange={handleModeChange}
                verifiedCount={verifiedCount}
            />
            {mode === "random" ? (
                <RandomModeView
                    onVerifiedCountChange={handleVerifiedCountChange}
                />
            ) : (
                <SequentialModeView
                    onVerifiedCountChange={handleVerifiedCountChange}
                />
            )}
        </div>
    );
};
