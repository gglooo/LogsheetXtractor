using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;

using Microsoft.Extensions.Logging;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record ResetLogsheetProofreadingCommand(
    Guid LogsheetId
);

public static class ResetLogsheetProofreadingHandler
{
    public static async Task<Result> Handle(
        ResetLogsheetProofreadingCommand request,
        IAppDbContext dbContext,
        ILogger<ResetLogsheetProofreadingCommand> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Resetting proofreading for Logsheet {LogsheetId}", request.LogsheetId);

        var logsheet = dbContext.Logsheets
            .Include(l => l.ExtractedValues)
            .FirstOrDefault(l => l.Id == request.LogsheetId);

        if (logsheet is null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found", request.LogsheetId);
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }

        if (!logsheet.CanBeReset())
        {
            logger.LogWarning("Cannot reset proofreading for Logsheet {LogsheetId} because it is currently processing.", request.LogsheetId);
            return Result.Fail(new InvalidStateError("Logsheet must finish processing before resetting proofreading."));
        }

        logsheet.Status = ELogSheetStatus.Pending;
        logsheet.ProcessedAt = null;
        logsheet.CompletedAt = null;

        if (logsheet.ExtractedValues.Any())
        {
            dbContext.ExtractedValues.RemoveRange(logsheet.ExtractedValues);
        }

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Proofreading reset successfully for Logsheet {LogsheetId}", request.LogsheetId);
        return new Result();
    }
}