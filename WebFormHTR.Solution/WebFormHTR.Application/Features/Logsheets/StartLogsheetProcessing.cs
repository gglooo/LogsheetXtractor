using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Extensions;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;
using Wolverine;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record StartLogsheetProcessingCommand(Guid LogsheetId);

public static class StartLogsheetProcessingHandler
{
    public static async Task<Result> Handle(StartLogsheetProcessingCommand command,
        IAppDbContext dbContext,
        IMessageBus bus,
        ICredentialCookieAccessor credentialCookieAccessor,
        ILogger<StartLogsheetProcessingCommand> logger, CancellationToken ct)
    {
        logger.LogInformation("Starting logsheet processing for logsheet {LogsheetId}", command.LogsheetId);

        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == command.LogsheetId, ct);
        if (logsheet == null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found", command.LogsheetId);
            return Result.Fail(new NotFoundError("Logsheet {LogsheetId} not found"));
        }

        if (!logsheet.CanBeProcessed())
        {
            logger.LogWarning(
                "Logsheet {LogsheetId} is not in a valid state for processing to be initiated. Status: {Status}",
                command.LogsheetId, logsheet.Status);
            return Result.Fail(new InvalidStateError(
                "Logsheet is not in a valid state for processing to be initiated."));
        }

        logsheet.Status = ELogSheetStatus.Processing;
        await bus.PublishWithContextAsync(new ProcessLogsheetDataCommand(command.LogsheetId), credentialCookieAccessor);

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}