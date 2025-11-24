using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.ROIs.DTOs;

public record SetRoiDto
(
    Guid? Id,
    string VariableName,
    ERoiType Type,
    Coordinates Coordinates
);