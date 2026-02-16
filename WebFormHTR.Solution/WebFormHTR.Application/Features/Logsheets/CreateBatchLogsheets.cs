using FluentResults;
using Microsoft.Extensions.Logging;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record BatchCreateLogsheetCommand(
    Guid TemplateId,
    Guid? BacksideTemplateId,
    Guid[] FileIds
);

public static class CreateBatchLogsheetsHandler
{
    public static async Task<Result<IEnumerable<LogsheetDetailDto>>> Handle(
        BatchCreateLogsheetCommand request,
        CancellationToken ct,
        IAppDbContext dbContext,
        IMapper mapper,
        ILogger<BatchCreateLogsheetCommand> logger)
    {
        var fileIds = request.FileIds.ToList();
        logger.LogInformation("Starting batch logsheet creation for {Count} files with Template {TemplateId}", fileIds.Count, request.TemplateId);

        if (!fileIds.Any())
        {
            logger.LogWarning("No files provided for batch logsheet creation");
            return Result.Ok<IEnumerable<LogsheetDetailDto>>(new List<LogsheetDetailDto>());
        }

        var files = await dbContext.Files
            .Where(f => fileIds.Contains(f.Id))
            .Select(f => f.Id)
            .ToListAsync(ct);

        if (files.Count != fileIds.Count)
        {
            logger.LogWarning("One or more files not found during batch logsheet creation");
            return Result.Fail(new NotFoundError("One or more files not found"));
        }

        var existingAssignments = await dbContext.Logsheets
            .Where(l => fileIds.Contains(l.FileId))
            .AnyAsync(ct);

        if (existingAssignments)
        {
            logger.LogWarning("One or more files are already assigned to a logsheet");
            return Result.Fail(new ConstraintError("One or more files are already assigned to a logsheet"));
        }

        var templates = await dbContext.Templates
            .Include(t => t.Rois)
            .Where(t => t.Id == request.TemplateId || t.Id == request.BacksideTemplateId)
            .ToListAsync(ct);

        if (templates.Count != (request.BacksideTemplateId.HasValue ? 2 : 1))
        {
            logger.LogWarning("One or more templates not found during batch logsheet creation");
            return Result.Fail(new NotFoundError("One or more templates not found"));
        }

        var logsheets = mapper.Map<IList<Logsheet>>(request.FileIds.Select((fileId) =>
            new CreateLogsheetCommand(
                request.TemplateId,
                request.BacksideTemplateId,
                fileId
            )));
            
        if (logsheets is null)
        {
            logger.LogError("Failed to map logsheets from command");
            return Result.Fail(new Error("Failed to map logsheets"));
        }

        await dbContext.Logsheets.AddRangeAsync(logsheets, ct);
        await dbContext.SaveChangesAsync(ct);

        var newIds = logsheets.Select(x => x.Id).ToList();

        var resultEntities = await dbContext.Logsheets
            .AsNoTracking()
            .Include(l => l.Template)
            .ThenInclude(t => t.Rois)
            .Include(l => l.File)
            .Include(l => l.ExtractedValues)
            .ThenInclude(e => e.Roi)
            .Where(l => newIds.Contains(l.Id))
            .ToListAsync(ct);

        var resultDtos = mapper.Map<IEnumerable<LogsheetDetailDto>>(resultEntities);

        logger.LogInformation("Successfully created {Count} logsheets", resultDtos.Count());
        return Result.Ok<IEnumerable<LogsheetDetailDto>>(resultDtos);
    }
}