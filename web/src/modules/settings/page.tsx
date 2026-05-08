import {
    supportedLanguages,
    type Language,
} from "@/components/language-context";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { Switch } from "@/components/ui/switch";
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from "@/components/ui/tooltip";
import { useLanguage } from "@/components/use-language";
import { useNavigateUp } from "@/hooks/use-navigate-up";
import { useUserSettings } from "@/modules/settings/hooks/useUserSettings";
import { ArrowLeft, Info, Trash2 } from "lucide-react";
import { useIntl } from "react-intl";
import { toast } from "sonner";
import { CredentialsAction } from "./actions/credentials-action";
import { useCredentialsStatus, useDeleteCredentialsMutation } from "./api";

export const SettingsPage = () => {
    const intl = useIntl();
    const navigateUp = useNavigateUp();
    const { locale, setLocale } = useLanguage();
    const { data: status, isLoading } = useCredentialsStatus();
    const deleteMutation = useDeleteCredentialsMutation();

    const { userSettings, setUserSettings } = useUserSettings();

    const handleClear = async () => {
        try {
            await deleteMutation.mutateAsync();
            toast.success(
                intl.formatMessage({
                    id: "settings.credentials.clear.success",
                    defaultMessage: "Personal credentials cleared.",
                }),
            );
        } catch (error) {
            console.error("Failed to clear credentials", error);
            toast.error(
                intl.formatMessage({
                    id: "settings.credentials.clear.error",
                    defaultMessage: "Failed to clear credentials.",
                }),
            );
        }
    };

    return (
        <main className="p-10 max-w-4xl mx-auto flex flex-col gap-10">
            <div>
                <Button
                    variant="ghost"
                    className="mb-6 -ml-4 text-muted-foreground"
                    onClick={() => navigateUp()}
                >
                    <ArrowLeft className="h-4 w-4 mr-2" />
                    {intl.formatMessage({
                        id: "common.goBack",
                        defaultMessage: "Go back",
                    })}
                </Button>
                <h1 className="text-3xl font-bold tracking-tight">
                    {intl.formatMessage({
                        id: "settings.title",
                        defaultMessage: "Settings",
                    })}
                </h1>
                <p className="text-muted-foreground mt-2">
                    {intl.formatMessage({
                        id: "settings.description",
                        defaultMessage:
                            "Manage your personal preferences and system configurations.",
                    })}
                </p>
            </div>

            <div className="flex flex-col gap-4 border p-6 rounded-lg bg-card text-card-foreground shadow-sm">
                <div className="flex justify-between items-start">
                    <div className="flex flex-col gap-2">
                        <h2 className="text-xl font-semibold">
                            {intl.formatMessage({
                                id: "settings.status.title",
                                defaultMessage: "Current configuration status",
                            })}
                        </h2>
                        {isLoading ? (
                            <Spinner />
                        ) : status?.hasUserCredentials ? (
                            <div className="flex items-center gap-2 mt-1">
                                <Badge
                                    variant="default"
                                    className="bg-blue-600 hover:bg-blue-700"
                                >
                                    {intl.formatMessage({
                                        id: "settings.status.personal",
                                        defaultMessage: "Using personal keys",
                                    })}
                                </Badge>
                                <span className="text-sm text-muted-foreground">
                                    {intl.formatMessage({
                                        id: "settings.status.personal.desc",
                                        defaultMessage:
                                            "The OCR system is using the personal credentials you provided.",
                                    })}
                                </span>
                            </div>
                        ) : status?.available ? (
                            <div className="flex items-center gap-6">
                                <Badge
                                    variant="default"
                                    className="bg-green-600 hover:bg-green-700"
                                >
                                    {intl.formatMessage({
                                        id: "settings.status.server",
                                        defaultMessage:
                                            "System defaults active",
                                    })}
                                </Badge>
                                <span className="text-sm text-muted-foreground">
                                    {intl.formatMessage({
                                        id: "settings.status.server.desc",
                                        defaultMessage:
                                            "The system is ready to process logsheets using centrally provided services.",
                                    })}
                                </span>
                            </div>
                        ) : (
                            <div className="flex items-center gap-2 mt-1">
                                <Badge variant="destructive">
                                    {intl.formatMessage({
                                        id: "settings.status.missing",
                                        defaultMessage: "Missing credentials",
                                    })}
                                </Badge>
                                <span className="text-sm text-muted-foreground">
                                    {intl.formatMessage({
                                        id: "settings.status.missing.desc",
                                        defaultMessage:
                                            "The server does not have default credentials configured. You must provide personal keys below.",
                                    })}
                                </span>
                            </div>
                        )}
                    </div>
                </div>
            </div>

            <div className="border p-6 rounded-lg bg-card shadow-sm">
                <CredentialsAction />

                <div className="mt-8 border-t pt-6">
                    <h4 className="text-sm font-medium mb-3">
                        {intl.formatMessage({
                            id: "settings.dangerZone",
                            defaultMessage: "Danger zone",
                        })}
                    </h4>
                    <Button
                        variant="destructive"
                        onClick={handleClear}
                        disabled={deleteMutation.isPending}
                    >
                        <Trash2 className="h-4 w-4 mr-2" />
                        {intl.formatMessage({
                            id: "settings.credentials.clear",
                            defaultMessage: "Clear personal credentials",
                        })}
                    </Button>
                </div>
            </div>

            <div className="flex flex-col gap-4 border p-6 rounded-lg bg-card text-card-foreground shadow-sm">
                <div className="flex justify-between items-start">
                    <div className="flex flex-col gap-2">
                                <h2 className="text-xl font-semibold">
                                    {intl.formatMessage({
                                        id: "settings.user.title",
                                        defaultMessage: "User settings",
                                    })}
                                </h2>
                                <span className="text-sm text-muted-foreground mb-2">
                                    {intl.formatMessage({
                                        id: "settings.user.description",
                                        defaultMessage:
                                            "Configure personal behavior preferences",
                                    })}
                                </span>

                        <div className="w-full max-w-xs flex flex-col gap-4">
                            <div className="flex items-center gap-6">
                                <Switch
                                    id="uglyCheckboxes"
                                    checked={userSettings.uglyCheckboxes}
                                    onCheckedChange={(checked) =>
                                        setUserSettings({
                                            ...userSettings,
                                            uglyCheckboxes: checked,
                                        })
                                    }
                                />
                                <Label htmlFor="uglyCheckboxes">
                                    {intl.formatMessage({
                                        id: "settings.user.uglyCheckboxes",
                                        defaultMessage: "Ugly checkboxes",
                                    })}
                                </Label>
                                <Tooltip>
                                    <TooltipTrigger>
                                        <Info className="h-4 w-4" />
                                    </TooltipTrigger>
                                    <TooltipContent className="max-w-xs">
                                        <p>
                                            {intl.formatMessage({
                                                id: "settings.user.uglyCheckboxes.desc",
                                                defaultMessage:
                                                    "Enable when processing logsheets with checkboxes of irregular shape or large edges. Disabled by default.",
                                            })}
                                        </p>
                                    </TooltipContent>
                                </Tooltip>
                            </div>
                            <div className="flex items-center gap-6">
                                <Switch
                                    id="automaticAlignmentOnUpload"
                                    checked={
                                        userSettings.automaticAlignmentOnUpload
                                    }
                                    onCheckedChange={(checked) =>
                                        setUserSettings({
                                            ...userSettings,
                                            automaticAlignmentOnUpload: checked,
                                        })
                                    }
                                />
                                <Label htmlFor="automaticAlignmentOnUpload">
                                    {intl.formatMessage({
                                        id: "settings.user.automaticAlignmentOnUpload",
                                        defaultMessage:
                                            "Automatic alignment on upload",
                                    })}
                                </Label>
                                <Tooltip>
                                    <TooltipTrigger>
                                        <Info className="h-4 w-4" />
                                    </TooltipTrigger>
                                    <TooltipContent className="max-w-xs">
                                        <p>
                                            {intl.formatMessage({
                                                id: "settings.user.automaticAlignmentOnUpload.desc",
                                                defaultMessage:
                                                    "Automatically align logsheets right after upload. Enabled by default.",
                                            })}
                                        </p>
                                    </TooltipContent>
                                </Tooltip>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="flex flex-col gap-4 border p-6 rounded-lg bg-card text-card-foreground shadow-sm">
                <div className="flex justify-between items-start">
                    <div className="flex flex-col gap-2">
                        <h2 className="text-xl font-semibold">
                            {intl.formatMessage({
                                id: "settings.language.title",
                                defaultMessage: "Language",
                            })}
                        </h2>
                        <span className="text-sm text-muted-foreground mb-2">
                            {intl.formatMessage({
                                id: "settings.language.description",
                                defaultMessage:
                                    "Select your preferred language",
                            })}
                        </span>

                        <div className="w-full max-w-xs">
                            <Select
                                value={locale}
                                onValueChange={(value) =>
                                    setLocale(value as Language)
                                }
                            >
                                <SelectTrigger>
                                    <SelectValue placeholder="Select language" />
                                </SelectTrigger>
                                <SelectContent>
                                    {supportedLanguages.map((lang) => (
                                        <SelectItem
                                            key={lang.code}
                                            value={lang.code}
                                        >
                                            {lang.label}
                                        </SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>
                    </div>
                </div>
            </div>
        </main>
    );
};
