using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;
using Wolverine;

namespace WebFormHTR.Application.Features.Logsheets;

public record StartBatchLogsheetProcessingCommand(Guid[] LogsheetIds);

public static class StartBatchLogsheetProcessingHandler
{
    public static async Task Handle(StartBatchLogsheetProcessingCommand command, IAppDbContext dbContext,
        IMessageBus bus,
        ILogger<StartBatchLogsheetProcessingCommand> logger, CancellationToken ct)
    {
        logger.LogInformation("Starting batch processing for {Count} logsheets", command.LogsheetIds.Length);

        var logsheetsToProcess = await dbContext.Logsheets
            .Where(ls => command.LogsheetIds.AsEnumerable().Contains(ls.Id))
            .ToListAsync(ct);

        var validLogsheetsToProcess = new List<Guid>();

        foreach (var logsheet in logsheetsToProcess)
        {
            if (!logsheet.CanBeProcessed())
            {
                logger.LogWarning("Logsheet {LogsheetId} is not in a valid state to start processing. Status: {Status}",
                    logsheet.Id, logsheet.Status);
                continue;
            }

            logsheet.Status = ELogSheetStatus.Processing;
            validLogsheetsToProcess.Add(logsheet.Id);
        }

        if (validLogsheetsToProcess.Count == 0)
        {
            logger.LogWarning("No valid logsheets found in the batch command to start processing.");
            return;
        }

        await bus.PublishAsync(new BatchProcessLogsheetDataCommand(validLogsheetsToProcess.ToArray()));

        await dbContext.SaveChangesAsync(ct);
    }
}