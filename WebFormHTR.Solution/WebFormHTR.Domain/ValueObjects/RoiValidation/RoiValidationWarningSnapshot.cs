namespace WebFormHTR.Domain.ValueObjects.RoiValidation;

public sealed record RoiValidationWarningSnapshot(
    string Code,
    string Message,
    string Path
);
