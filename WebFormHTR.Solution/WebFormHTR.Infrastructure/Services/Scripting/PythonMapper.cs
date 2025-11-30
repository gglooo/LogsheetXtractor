using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public static class PythonMapper
{
    private static Domain.ValueObjects.Coordinates MapCoordinates(IList<float> coords)
    {
        return new Domain.ValueObjects.Coordinates
        {
            X = coords[0],
            Y = coords[1],
            Width = coords[2] - coords[0],
            Height = coords[3] - coords[1]
        };
    }

    public static SelectRoisOutputDto ToSelectRoisOutputDtoList(this PythonSelectRoisOutputDto outputDto,
        Guid templateId)
    {
        var rois = new List<RoiDto>();
        var residuals = new List<ResidualDto>();

        foreach (var roi in outputDto.Content)
        {
            if (roi.Coords.Count != 4)
            {
                continue;
            }

            var coordinates = MapCoordinates(roi.Coords);

            if (!Enum.TryParse(roi.Type, true, out ERoiType roiType))
            {
                roiType = ERoiType.Text;
            }

            var roiDto = new RoiDto(
                null,
                roi.VarName ?? "Unnamed",
                templateId,
                roiType,
                coordinates
            );

            rois.Add(roiDto);
        }

        foreach (var residual in outputDto.ToIgnore)
        {
            if (residual.Coords.Count != 4)
            {
                continue;
            }

            var coordinates = MapCoordinates(residual.Coords);
            var residualDto = new ResidualDto(
                null,
                templateId,
                residual.Content ?? string.Empty,
                coordinates
            );

            residuals.Add(residualDto);
        }

        return new SelectRoisOutputDto(rois, residuals);
    }
}