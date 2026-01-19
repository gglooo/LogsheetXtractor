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
    public static async Task<Result<LogsheetDetailDto>> Handle(
        ResetLogsheetProofreadingCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var logsheet = dbContext.Logsheets
            .Include(l => l.ExtractedValues)
            .FirstOrDefault(l => l.Id == request.LogsheetId);
        if (logsheet is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        logsheet.Status = ELogSheetStatus.Pending;
        logsheet.ProcessedAt = null;
        logsheet.CompletedAt = null;

        if (logsheet.ExtractedValues.Any())
        {
            dbContext.ExtractedValues.RemoveRange(logsheet.ExtractedValues);
        }

        await dbContext.SaveChangesAsync(ct);

        return mapper.Map<LogsheetDetailDto>(logsheet);
    }
}