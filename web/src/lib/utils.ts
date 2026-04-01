import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

const sizes = ["Bytes", "KB", "MB", "GB", "TB"];
const k = 1024;

export const formatFileSize = (bytes: number) => {
    if (bytes === 0) {
        return "0 Bytes";
    }
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
};

export const getUrlFromBytes = (bytes: ArrayBuffer) => {
    return URL.createObjectURL(new Blob([bytes]));
};

export const getLocalDate = (value: Date | string) => {
    if (value instanceof Date) {
        return value;
    }

    const hasTimezoneInfo = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(value);
    return new Date(hasTimezoneInfo ? value : `${value}Z`);
};
