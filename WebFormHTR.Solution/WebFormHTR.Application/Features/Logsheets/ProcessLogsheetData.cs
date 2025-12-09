using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record ProcessLogsheetDataCommand(Guid LogsheetId);

public static class ProcessLogsheetDataHandler
{
    public static async Task<Result> Handle(ProcessLogsheetDataCommand request, IAppDbContext dbContext,
        IHtrScriptEngine scriptEngine, CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == request.LogsheetId);
        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }

        try
        {
            await scriptEngine.ProcessLogsheetAsync(new ProcessLogsheetInputDto(logsheet), ct);
            // await dbContext.SaveChangesAsync(ct);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to process logsheet data: {ex.Message}");
        }
    }
}