using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Extensions;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;
using Wolverine;

namespace WebFormHTR.Application.Features.Logsheets;

public record StartBatchLogsheetProcessingCommand(Guid[] LogsheetIds, ProcessLogsheetDataOptions? options);

public static class StartBatchLogsheetProcessingHandler
{
    public static async Task<Result> Handle(StartBatchLogsheetProcessingCommand command, IAppDbContext dbContext,
        IMessageBus bus,
        ICredentialCookieAccessor credentialCookieAccessor,
        ILogger<StartBatchLogsheetProcessingCommand> logger, CancellationToken ct)
    {
        logger.LogInformation("Starting batch processing for {Count} logsheets", command.LogsheetIds.Length);

        var logsheetsToProcess = await dbContext.Logsheets
            .Where(ls => command.LogsheetIds.AsEnumerable().Contains(ls.Id))
            .ToListAsync(ct);

        var anyLogsheetsProcessed = false;
        foreach (var logsheet in logsheetsToProcess)
        {
            if (!logsheet.CanBeProcessed())
            {
                logger.LogWarning("Logsheet {LogsheetId} is not in a valid state to start processing. Status: {Status}",
                    logsheet.Id, logsheet.Status);
                continue;
            }

            logsheet.Status = ELogSheetStatus.Processing;

            await bus.PublishWithContextAsync(
                new ProcessLogsheetDataCommand(logsheet.Id, command.options),
                credentialCookieAccessor);

            anyLogsheetsProcessed = true;
        }

        if (!anyLogsheetsProcessed)
        {
            logger.LogWarning("No valid logsheets found in the batch command to start processing.");
            return Result.Fail(new InvalidStateError("No valid logsheets found to start processing."));
        }

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}