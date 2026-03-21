using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Enums;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace LogsheetXtractor.Application.Features.Logsheets;

public record AlignLogsheetCommand(Guid LogsheetId);

public static class AlignLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(
        AlignLogsheetCommand request,
        IAppDbContext dbContext,
        ILogsheetService logsheetService,
        IMessageBus bus,
        ILogger<AlignLogsheetCommand> logger,
        CancellationToken ct
    )
    {
        logger.LogInformation(
            "Starting automatic alignment for Logsheet {LogsheetId}",
            request.LogsheetId
        );

        var logsheet = await dbContext.Logsheets.FindAsync(request.LogsheetId, ct);
        if (logsheet is null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found for alignment", request.LogsheetId);
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        var shouldResetStatus = logsheet.Status == ELogSheetStatus.Aligning;

        var alignmentResult = await logsheetService.AlignLogsheetAsync(logsheet, ct);
        if (shouldResetStatus)
        {
            logsheet.Status = ELogSheetStatus.Pending;
        }

        if (alignmentResult.IsFailed)
        {
            if (shouldResetStatus)
            {
                await dbContext.SaveChangesAsync(ct);
            }

            return alignmentResult;
        }

        await bus.PublishAsync(new LogsheetAutomaticallyAlignedEvent(request.LogsheetId));
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "Automatic alignment completed successfully for Logsheet {LogsheetId}",
            request.LogsheetId
        );

        return alignmentResult;
    }
}
