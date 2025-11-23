using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets.DTOs;

public record LogsheetListDto
(
    Guid Id,
    Guid TemplateId,
    Guid FileId,
    ELogSheetStatus Status,
    DateTime? ProcessedAt
);