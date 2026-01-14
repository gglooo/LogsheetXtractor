using FluentResults;
using MapsterMapper;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;

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
        CancellationToken ct)
    {
        var logsheet = dbContext.Logsheets.FirstOrDefault(l => l.Id == request.LogsheetId);
        if (logsheet is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        var areAllValuesVerified = dbContext.ExtractedValues
            .Where(ev => ev.LogsheetId == logsheet.Id)
            .All(ev => ev.Status == EVerificationStatus.Verified);

        if (logsheet.Status != ELogSheetStatus.NeedsReview || !areAllValuesVerified)
        {
            return Result.Fail(
                new InvalidStateError("Logsheet is not in a state that allows completing proofreading."));
        }

        logsheet.Status = ELogSheetStatus.Completed;
        await dbContext.SaveChangesAsync(ct);

        return mapper.Map<LogsheetDetailDto>(logsheet);
    }
}