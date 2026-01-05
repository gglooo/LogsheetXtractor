import type { LogsheetAlignmentData } from "@/modules/logsheets/schema";
import {
    UploadLogsheetsContext,
    type ContextLogsheet,
} from "@/modules/logsheets/upload-logsheets/hooks/use-upload-logsheets-context";
import { useMemo, useRef, useState, type ReactNode } from "react";
import { useLocation, useNavigate } from "react-router-dom";

const UPLOAD_FLOW_STEPS = ["upload", "align", "review"];

export const UploadLogsheetsProvider = ({
    children,
}: {
    children: ReactNode;
}) => {
    const [logsheets, setLogsheets] = useState<ContextLogsheet[]>([]);
    const navigate = useNavigate();
    const location = useLocation();

    const currentStepIndex = useMemo(() => {
        const foundIndex = UPLOAD_FLOW_STEPS.findIndex((step) =>
            location.pathname.endsWith(`/${step}`)
        );

        return foundIndex !== -1 ? foundIndex : 0;
    }, [location.pathname]);

    const canContinue = logsheets.length > 0;

    const addLogsheets = (files: File[]) => {
        const newLogsheets = files.map((file) => ({
            rawFile: file,
        }));

        setLogsheets((prev) => [...prev, ...newLogsheets]);
    };

    const removeLogsheet = (index: number) => {
        setLogsheets((prev) => prev.filter((_, i) => i !== index));
    };

    const setAlignment = (id: string, alignment: LogsheetAlignmentData) => {
        setLogsheets((prev) =>
            prev.map((l) =>
                l.file?.id === id ? { ...l, alignmentData: alignment } : l
            )
        );
    };

    const clearLogsheets = () => {
        setLogsheets([]);
    };

    const nextHandlerRef = useRef<() => Promise<boolean | void>>(undefined);

    const registerNextHandler = (handler: () => Promise<boolean | void>) => {
        nextHandlerRef.current = handler;

        return () => {
            if (nextHandlerRef.current === handler) {
                nextHandlerRef.current = undefined;
            }
        };
    };

    const handleContinue = async () => {
        if (!canContinue) return;

        if (nextHandlerRef.current) {
            const shouldContinue = await nextHandlerRef.current();
            if (shouldContinue === false) return;
        }

        const isLastStep = currentStepIndex === UPLOAD_FLOW_STEPS.length - 1;

        if (!isLastStep) {
            const nextStep = UPLOAD_FLOW_STEPS[currentStepIndex + 1];

            const currentPath = location.pathname;
            const currentStepSuffix = UPLOAD_FLOW_STEPS[currentStepIndex];

            const flowRoot =
                currentStepIndex === 0
                    ? currentPath
                    : currentPath.replace(
                          new RegExp(`/${currentStepSuffix}$`),
                          ""
                      );

            const cleanRoot = flowRoot.replace(/\/$/, "");
            navigate(`${cleanRoot}/${nextStep}`);
        } else {
            if (!nextHandlerRef.current) {
                await submitData();
            }
        }
    };

    const submitData = async () => {
        try {
            console.log("Submitting payload:", logsheets);

            clearLogsheets();
        } catch (error) {
            console.error("Submission failed", error);
        }
    };

    return (
        <UploadLogsheetsContext.Provider
            value={{
                logsheets,
                addLogsheets,
                setLogsheets,
                removeLogsheet,
                setAlignment,
                clearLogsheets,
                handleContinue,
                canContinue,
                registerNextHandler,
                submitLogsheets: submitData,
            }}
        >
            {children}
        </UploadLogsheetsContext.Provider>
    );
};
