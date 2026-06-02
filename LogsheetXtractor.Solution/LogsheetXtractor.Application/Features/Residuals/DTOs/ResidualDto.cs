using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

/// <summary>
/// Residual text/area metadata detected on a template outside ROI fields.
/// </summary>
public record ResidualDto(
    Guid? Id,
    Guid TemplateId,
    string Content,
    Coordinates Coordinates,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
