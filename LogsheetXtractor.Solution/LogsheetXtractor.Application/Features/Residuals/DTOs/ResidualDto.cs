using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

public record ResidualDto(
    Guid? Id,
    Guid TemplateId,
    string Content,
    Coordinates Coordinates,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
