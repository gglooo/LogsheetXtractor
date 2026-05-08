export const ShortcutLabel = ({
    label,
    shortcut,
}: {
    label: string;
    shortcut: string;
}) => {
    const index = label.toLowerCase().indexOf(shortcut.toLowerCase());

    if (index === -1) return <>{label}</>;

    return (
        <span>
            {label.substring(0, index)}
            <span className="underline decoration-1 underline-offset-2">
                {label.charAt(index)}
            </span>
            {label.substring(index + 1)}
        </span>
    );
};
