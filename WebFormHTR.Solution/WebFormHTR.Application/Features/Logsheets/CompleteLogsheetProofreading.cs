using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record CompleteLogsheetProofreadingCommand(
    Guid LogsheetId
);

public static class CompleteLogsheetProofreadingHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(
        CompleteLogsheetProofreadingCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        ILogger<CompleteLogsheetProofreadingCommand> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Completing proofreading for Logsheet {LogsheetId}", request.LogsheetId);

        var logsheet = dbContext.Logsheets
            .Include(l => l.ExtractedValues)
            .ThenInclude(eV => eV.Roi)
            .FirstOrDefault(l => l.Id == request.LogsheetId);
        if (logsheet is null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found", request.LogsheetId);
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        var areAllValuesVerified = dbContext.ExtractedValues
            .Where(ev => ev.LogsheetId == logsheet.Id)
            .All(ev => ev.Status == EVerificationStatus.Verified);

        if (logsheet.Status != ELogSheetStatus.NeedsReview || !areAllValuesVerified)
        {
            logger.LogWarning(
                "Proofreading completion failed validation for Logsheet {LogsheetId}. Status: {Status}, AllValuesVerified: {Verified}",
                request.LogsheetId, logsheet.Status, areAllValuesVerified);
            return Result.Fail(
                new InvalidStateError("Logsheet is not in a state that allows completing proofreading."));
        }

        logsheet.Status = ELogSheetStatus.Completed;
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Proofreading completed successfully for Logsheet {LogsheetId}", request.LogsheetId);
        return mapper.Map<LogsheetDetailDto>(logsheet);
    }
}