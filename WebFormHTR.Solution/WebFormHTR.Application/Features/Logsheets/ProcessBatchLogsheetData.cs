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

public sealed record BatchProcessLogsheetDataCommand(Guid[] LogsheetIds);

public static class ProcessBatchLogsheetDataHandler
{
    public static async Task<Result<IEnumerable<LogsheetDetailDto>>> Handle(BatchProcessLogsheetDataCommand request,
        IAppDbContext dbContext,
        ILogsheetService logsheetService,
        IHtrScriptEngine scriptEngine, IMapper mapper, CancellationToken ct)
    {
        var logsheets = await dbContext.Logsheets
            .Where(ls => request.LogsheetIds.Contains(ls.Id))
            .ToListAsync(ct);

        try
        {
            var processedLogsheets = await logsheetService.ProcessLogsheetsAsync(logsheets, ct);

            await dbContext.SaveChangesAsync(ct);

            return Result.Ok(processedLogsheets);
        }
        catch (Exception ex)
        {
            return Result.Fail<IEnumerable<LogsheetDetailDto>>($"Failed to process batch logsheet data: {ex.Message}");
        }
    }
}