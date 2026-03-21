using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Application.Features.Logsheets;

public sealed record SetLogsheetAlignmentCommand(Guid LogsheetId, AlignmentDataDto AlignmentData);

public static class SetLogsheetAlignmentHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(
        SetLogsheetAlignmentCommand command,
        IAppDbContext dbContext,
        IMapper mapper,
        ILogger<SetLogsheetAlignmentCommand> logger,
        CancellationToken ct
    )
    {
        logger.LogInformation(
            "Setting manual alignment for Logsheet {LogsheetId}",
            command.LogsheetId
        );

        var logsheet = await dbContext
            .Logsheets.Include(l => l.Template)
            .ThenInclude(t => t.Rois)
            .Include(l => l.ExtractedValues)
            .ThenInclude(e => e.Roi)
            .FirstOrDefaultAsync(ls => ls.Id == command.LogsheetId, ct);

        if (logsheet is null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found", command.LogsheetId);
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        if (!logsheet.CanBeEdited())
        {
            logger.LogWarning(
                "Logsheet {LogsheetId} is already processed or is currently processing (Status: {Status}) and cannot be re-aligned.",
                command.LogsheetId,
                logsheet.Status
            );
            return Result.Fail<LogsheetDetailDto>(
                new InvalidStateError(
                    "Logsheet is already processed or is currently processing and cannot be re-aligned."
                )
            );
        }

        logsheet.AlignmentData = mapper.Map<AlignmentContainer>(command.AlignmentData);

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "Manual alignment set successfully for Logsheet {LogsheetId}",
            command.LogsheetId
        );
        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}
