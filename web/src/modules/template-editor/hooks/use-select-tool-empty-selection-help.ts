import { DRAW_TOOL_KEY } from "@/modules/template-editor/hooks/shortcuts/types";
import type { EditorMode } from "@/modules/template-editor/hooks/use-template-editor";
import { useCallback, useEffect, useRef } from "react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

type UseSelectToolEmptySelectionHelpOptions = {
    mode: EditorMode;
    attemptsBeforeWarning?: number;
};

export const useSelectToolEmptySelectionHelp = ({
    mode,
    attemptsBeforeWarning = 2,
}: UseSelectToolEmptySelectionHelpOptions) => {
    const intl = useIntl();
    const emptySelectionAttemptsRef = useRef(0);
    const didShowWarningRef = useRef(false);

    useEffect(() => {
        if (mode !== "select") {
            emptySelectionAttemptsRef.current = 0;
            didShowWarningRef.current = false;
        }
    }, [mode]);

    const trackSelectionResult = useCallback(
        (selectedRoiCount: number) => {
            if (mode !== "select" || selectedRoiCount > 0) {
                emptySelectionAttemptsRef.current = 0;
                didShowWarningRef.current = false;
                return;
            }

            if (didShowWarningRef.current) {
                return;
            }

            emptySelectionAttemptsRef.current += 1;

            if (emptySelectionAttemptsRef.current < attemptsBeforeWarning) {
                return;
            }

            toast.warning(
                intl.formatMessage(
                    {
                        id: "template-editor.help.select-tool-empty-selection",
                        defaultMessage: `You are in Select mode. To create a new ROI, switch to the Draw tool in the tool panel or press "{key}".`,
                    },
                    { key: DRAW_TOOL_KEY.toUpperCase() },
                ),
            );

            didShowWarningRef.current = true;
        },
        [attemptsBeforeWarning, intl, mode],
    );

    return { trackSelectionResult };
};
