using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Template;

public sealed record DetectRoisCommand(Guid TemplateId);

public static class DetectRoisHandler
{
    public static async Task<Result<DetectRoisResponseDto>> Handle(
        DetectRoisCommand request,
        IRoiService roiService,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct
    )
    {
        var template = await dbContext
            .Templates.Include(t => t.Residuals)
            .FirstOrDefaultAsync(x => x.Id == request.TemplateId, ct);

        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        try
        {
            var detectedRoisResult = await roiService.DetectRoisAsync(template, ct);
            if (detectedRoisResult.IsFailed)
            {
                return detectedRoisResult.ToResult();
            }

            var detectedRois = detectedRoisResult.Value;

            template.Residuals.Clear();
            await dbContext.Residuals.AddRangeAsync(
                detectedRois.Residuals.Select(
                    mapper.Map<LogsheetXtractor.Domain.Entities.Residual>
                ),
                ct
            );

            await dbContext.SaveChangesAsync(ct);

            return Result.Ok(detectedRois);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}
