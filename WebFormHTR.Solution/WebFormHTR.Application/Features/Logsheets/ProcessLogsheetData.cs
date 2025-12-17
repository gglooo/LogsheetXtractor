using FluentResults;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record ProcessLogsheetDataCommand(Guid LogsheetId);

public static class ProcessLogsheetDataHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(ProcessLogsheetDataCommand request,
        IAppDbContext dbContext,
        IHtrScriptEngine scriptEngine, IMapper mapper, CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == request.LogsheetId, ct);
        var validationError = ValidateLogsheet(logsheet, true);
        if (validationError is not null)
        {
            return Result.Fail(validationError);
        }

        try
        {
            var output = await scriptEngine.ProcessLogsheetAsync(new ProcessLogsheetInputDto(logsheet!), ct);

            var extractedData = output.ExtractedData.BuildAdapter()
                .AddParameters("LogsheetId", request.LogsheetId)
                .AdaptToType<IEnumerable<ExtractedValue>>()
                .ToList();

            logsheet!.ExtractedValues.Clear();
            extractedData.ForEach(logsheet.ExtractedValues.Add);
            logsheet.ProcessedAt = DateTime.UtcNow;
            logsheet.Status = ELogSheetStatus.NeedsReview;

            await dbContext.SaveChangesAsync(ct);

            var updatedLogsheet = await dbContext.Logsheets
                .AsNoTracking()
                .Include(ls => ls.ExtractedValues)
                .ThenInclude(e => e.Roi)
                .FirstOrDefaultAsync(ls => ls.Id == logsheet.Id, ct);

            var logsheetDetailDto = mapper.Map<LogsheetDetailDto>(updatedLogsheet!);

            return Result.Ok(logsheetDetailDto);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to process logsheet data: {ex.Message}");
        }
    }

    private static Error? ValidateLogsheet(Logsheet? logsheet, bool debug)
    {
        if (logsheet is null)
        {
            return new NotFoundError("Logsheet not found");
        }

        // TODO: remove this
        if (debug)
        {
            return null;
        }

        if (logsheet.Status != ELogSheetStatus.Pending || logsheet.ProcessedAt is not null)
        {
            return new InvalidStateError("Logsheet is not in a valid state for processing");
        }

        return null;
    }
}