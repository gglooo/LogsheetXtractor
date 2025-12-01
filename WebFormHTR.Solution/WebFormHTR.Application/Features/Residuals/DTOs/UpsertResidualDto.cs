using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Residuals.DTOs;

public record UpsertResidualDto(
    Guid? Id,
    string Content,
    Coordinates Coordinates
);
