using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.ROIs.DTOs;

public record SetRoiDto(
    string? Id,
    string VariableName,
    ERoiType? Type,
    Coordinates Coordinates
);