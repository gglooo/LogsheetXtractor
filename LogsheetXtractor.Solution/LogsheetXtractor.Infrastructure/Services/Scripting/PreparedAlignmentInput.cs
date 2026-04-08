namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public sealed record PreparedAlignmentInput(
    bool IsAligned,
    string? AlignmentConfigPath,
    string? BacksideAlignmentConfigPath
);
