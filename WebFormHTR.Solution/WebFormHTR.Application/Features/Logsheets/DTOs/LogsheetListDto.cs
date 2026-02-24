using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets.DTOs;

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
