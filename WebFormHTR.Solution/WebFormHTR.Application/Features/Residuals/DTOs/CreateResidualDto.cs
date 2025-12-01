using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Residuals.DTOs;

public record CreateResidualDto(
    string Content,
    Coordinates Coordinates
);
