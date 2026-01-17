using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record GetAlignedRoisQuery(
    Guid LogsheetId,
    bool Frontside
);

public static class GetAlignedRoisHandler
{
    public static async Task<Result<IEnumerable<RoiDto>>> Handle(GetAlignedRoisQuery query, IMapper mapper,
        ICoordinateTransformerService coordinateTransformerService,
        IAppDbContext dbContext, CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets.Include(l => l.Template)
            .Include(l => l.BacksideTemplate)
            .ThenInclude(t => t.Rois)
            .AsNoTracking()
            .FirstOrDefaultAsync(ls => ls.Id == query.LogsheetId, ct);
        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }

        var alignmentData = query.Frontside
            ? logsheet.AlignmentDataModelConfig.Frontside
            : logsheet.AlignmentDataModelConfig.Backside;

        var template = query.Frontside ? logsheet.Template : logsheet.BacksideTemplate;

        if (template is null)
        {
            return Result.Fail(new NotFoundError("Logsheet template not found"));
        }

        if (alignmentData is null)
        {
            return Result.Ok(mapper.Map<IEnumerable<RoiDto>>(template?.Rois ?? []));
        }

        var globalPageCoords = coordinateTransformerService.TransformCoordinates(new Coordinates
        {
            X = 0,
            Y = 0,
            Width = template.Width ?? 0,
            Height = template.Height ?? 0
        }, new Coordinates
        {
            X = 0,
            Y = 0,
            Width = template.Width ?? 0,
            Height = template.Height ?? 0
        }, alignmentData);

        var scaleX = (double)template.Width! / globalPageCoords.Width;
        var scaleY = (double)template.Height! / globalPageCoords.Height;

        var roisToReturn = new List<Roi>();
        foreach (var roi in template.Rois)
        {
            var globalRoiCoords = coordinateTransformerService.TransformCoordinates(
                roi.Coordinates,
                new Coordinates { X = 0, Y = 0, Width = template.Width ?? 0, Height = template.Height ?? 0 },
                alignmentData
            );

            double relativeX = globalRoiCoords.X - globalPageCoords.X;
            double relativeY = globalRoiCoords.Y - globalPageCoords.Y;

            var finalCoordinates = new Coordinates
            {
                X = (int)Math.Round(relativeX * scaleX),
                Y = (int)Math.Round(relativeY * scaleY),
                Width = (int)Math.Round(globalRoiCoords.Width * scaleX),
                Height = (int)Math.Round(globalRoiCoords.Height * scaleY)
            };

            roi.Coordinates = finalCoordinates;
            roisToReturn.Add(roi);
        }

        return Result.Ok(mapper.Map<IEnumerable<RoiDto>>(roisToReturn));
    }
}