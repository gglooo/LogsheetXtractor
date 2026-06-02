using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Extensions;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace LogsheetXtractor.Application.Features.Logsheets;

/// <summary>
/// Command that starts asynchronous processing for a single logsheet.
/// </summary>
/// <param name="LogsheetId">The logsheet identifier to process.</param>
/// <param name="Options">Optional processing settings.</param>
public sealed record StartLogsheetProcessingCommand(
    Guid LogsheetId,
    ProcessLogsheetDataOptions? Options
);

/// <summary>
/// Validates processing prerequisites and enqueues logsheet processing.
/// </summary>
public static class StartLogsheetProcessingHandler
{
    /// <summary>
    /// Sets a logsheet to processing state and publishes the processing command with request context.
    /// This keeps HTTP request handling responsive while extraction runs in the background.
    /// </summary>
    public static async Task<Result> Handle(
        StartLogsheetProcessingCommand command,
        IAppDbContext dbContext,
        IMessageBus bus,
        ICredentialCookieAccessor credentialCookieAccessor,
        ILogger<StartLogsheetProcessingCommand> logger,
        CancellationToken ct
    )
    {
        logger.LogInformation(
            "Starting logsheet processing for logsheet {LogsheetId}",
            command.LogsheetId
        );

        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(
            ls => ls.Id == command.LogsheetId,
            ct
        );
        if (logsheet == null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found", command.LogsheetId);
            return Result.Fail(new NotFoundError("Logsheet {LogsheetId} not found"));
        }

        if (!logsheet.CanBeProcessed())
        {
            logger.LogWarning(
                "Logsheet {LogsheetId} is not in a valid state for processing to be initiated. Status: {Status}",
                command.LogsheetId,
                logsheet.Status
            );
            return Result.Fail(
                new InvalidStateError(
                    "Logsheet is not in a valid state for processing to be initiated."
                )
            );
        }

        logsheet.Status = ELogSheetStatus.Processing;
        await bus.PublishWithContextAsync(
            new ProcessLogsheetDataCommand(command.LogsheetId, command.Options),
            credentialCookieAccessor,
            ct
        );

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
