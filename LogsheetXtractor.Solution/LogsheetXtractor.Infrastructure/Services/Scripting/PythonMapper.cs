using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;

namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public static class PythonMapper
{
    private static LogsheetXtractor.Domain.ValueObjects.Coordinates MapCoordinates(
        IList<int> coords
    )
    {
        return new LogsheetXtractor.Domain.ValueObjects.Coordinates(
            coords[0],
            coords[1],
            coords[2] - coords[0],
            coords[3] - coords[1]
        );
    }

    public static SelectRoisOutputDto ToSelectRoisOutputDtoList(
        this PythonSelectRoisOutputDto outputDto,
        Guid templateId
    )
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

            ERoiType? parsedRoiType = Enum.TryParse(roi.Type, true, out ERoiType roiType)
                ? roiType
                : null;

            var roiDto = new RoiDto(
                null,
                roi.VarName ?? $"Unnamed-{Guid.NewGuid()}",
                templateId,
                parsedRoiType,
                coordinates,
                DateTime.UtcNow,
                null
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
                coordinates,
                DateTime.UtcNow,
                null
            );

            residuals.Add(residualDto);
        }

        return new SelectRoisOutputDto(rois, residuals);
    }
}
