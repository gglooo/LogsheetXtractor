using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public static class PythonMapper
{
    public static List<RoiDto> ToRoiDtoList(this PythonSelectRoisOutputDto outputDto, Guid templateId)
    {
        var rois = new List<RoiDto>();
        foreach (var roi in outputDto.Content)
        {
            if (roi.Coords.Count != 4)
            {
                continue;
            }

            var x = roi.Coords[0];
            var y = roi.Coords[1];
            var width = roi.Coords[2] - x;
            var height = roi.Coords[3] - y;

            if (!Enum.TryParse(roi.Type, true, out ERoiType roiType))
            {
                roiType = ERoiType.Text;
            }

            var roiDto = new RoiDto(
                null,
                roi.VarName ?? "Unnamed",
                templateId,
                roiType,
                new Domain.ValueObjects.Coordinates
                {
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                }
            );

            rois.Add(roiDto);
        }

        return rois;
    }
}