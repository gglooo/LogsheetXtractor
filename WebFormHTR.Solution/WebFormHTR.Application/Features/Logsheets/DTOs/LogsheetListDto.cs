using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets.DTOs;

public record LogsheetListDto(
    Guid Id,
    Guid TemplateId,
    Guid? BacksideTemplateId,
    FileDto File,
    ELogSheetStatus Status,
    DateTime? ProcessedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);