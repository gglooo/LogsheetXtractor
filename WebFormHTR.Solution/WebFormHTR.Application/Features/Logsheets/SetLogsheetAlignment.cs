using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record SetLogsheetAlignmentCommand(Guid LogsheetId, AlignmentDataDto AlignmentData);

public static class SetLogsheetAlignmentHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(SetLogsheetAlignmentCommand command,
        IAppDbContext dbContext, IMapper mapper, CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets
            .Include(l => l.Template)
            .ThenInclude(t => t.Rois)
            .Include(l => l.ExtractedValues)
            .ThenInclude(e => e.Roi)
            .FirstOrDefaultAsync(ls => ls.Id == command.LogsheetId, ct);

        if (logsheet is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        if (logsheet.Status == ELogSheetStatus.Completed || logsheet.Status == ELogSheetStatus.NeedsReview)
        {
            return Result.Fail<LogsheetDetailDto>(
                new InvalidStateError("Logsheet is already processed and cannot be re-aligned."));
        }

        logsheet.AlignmentDataModelConfig = mapper.Map<AlignmentContainer>(command.AlignmentData);

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}