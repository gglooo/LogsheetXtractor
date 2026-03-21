using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

public record LogsheetListDto(
    Guid Id,
    Guid TemplateId,
    FileDto File,
    ELogSheetStatus Status,
    bool IsFrontAligned,
    bool IsBackAligned,
    string? ErrorMessage,
    DateTime? ProcessedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
