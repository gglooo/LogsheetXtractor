using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Residuals.DTOs;

public record ResidualDto(
    Guid? Id,
    Guid TemplateId,
    string Content,
    Coordinates Coordinates,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);