using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.ROIs.DTOs;

public record CreateRoiDto(
    string VariableName,
    ERoiType Type,
    
    Coordinates Coordinates
);