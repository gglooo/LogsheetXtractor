using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;
using Wolverine;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record StartLogsheetProcessingCommand(Guid LogsheetId);

public static class StartLogsheetProcessingHandler
{
    public static async Task Handle(StartLogsheetProcessingCommand command,
        IAppDbContext dbContext,
        IMessageBus bus,
        ILogger<StartLogsheetProcessingCommand> logger, CancellationToken ct)
    {
        logger.LogInformation("Starting logsheet processing for logsheet {LogsheetId}", command.LogsheetId);

        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == command.LogsheetId, ct);
        if (logsheet == null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found", command.LogsheetId);
            return;
        }

        if (!logsheet.CanBeProcessed())
        {
            logger.LogWarning(
                "Logsheet {LogsheetId} is not in a valid state for processing to be initiated. Status: {Status}",
                command.LogsheetId, logsheet.Status);
            return;
        }

        logsheet.Status = ELogSheetStatus.Processing;
        await bus.PublishAsync(new ProcessLogsheetDataCommand(command.LogsheetId));

        await dbContext.SaveChangesAsync(ct);
    }
}