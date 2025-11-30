using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.ROIs.DTOs;

public record RoiDto(
    Guid? Id,
    string VariableName,
    Guid TemplateId,
    ERoiType? Type,
    Coordinates Coordinates
);