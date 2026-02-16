using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record ResetLogsheetProofreadingCommand(
    Guid LogsheetId
);

public static class ResetLogsheetProofreadingHandler
{
    public static async Task<Result> Handle(
        ResetLogsheetProofreadingCommand request,
        IAppDbContext dbContext,
        CancellationToken ct)
    {
        var logsheet = dbContext.Logsheets
            .Include(l => l.ExtractedValues)
            .FirstOrDefault(l => l.Id == request.LogsheetId);

        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }

        logsheet.Status = ELogSheetStatus.Pending;
        logsheet.ProcessedAt = null;
        logsheet.CompletedAt = null;

        if (logsheet.ExtractedValues.Any())
        {
            dbContext.ExtractedValues.RemoveRange(logsheet.ExtractedValues);
        }

        await dbContext.SaveChangesAsync(ct);

        return new Result();
    }
}