using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Application.Features.Logsheets.Events;
using Wolverine;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record BatchProcessLogsheetDataCommand(Guid[] LogsheetIds);

public static class ProcessBatchLogsheetDataHandler
{
    public static async Task Handle(BatchProcessLogsheetDataCommand request,
        IAppDbContext dbContext,
        IMessageBus bus,
        ILogsheetService logsheetService,
        CancellationToken ct)
    {
        var logsheets = await dbContext.Logsheets
            .Where(ls => request.LogsheetIds.AsEnumerable().Contains(ls.Id))
            .ToListAsync(ct);

        try
        {
            var processResult = await logsheetService.ProcessLogsheetsAsync(logsheets, ct);
            if (processResult.IsFailed)
            {
                var errorMessages = processResult.Errors.Select(e => e.Message).ToList();

                await bus.PublishAsync(new BatchProcessingFinishedEvent([], request.LogsheetIds,
                    errorMessages));
            }
            else
            {
                var processedLogsheets = processResult.Value.Select(l => l.Id).ToArray();
                var failedLogsheets = request.LogsheetIds.Except(processedLogsheets).ToArray();

                await bus.PublishAsync(new BatchProcessingFinishedEvent(processedLogsheets, failedLogsheets, []));
            }

            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            dbContext.ChangeTracker.Clear();

            await bus.PublishAsync(new BatchProcessingFinishedEvent([], request.LogsheetIds,
                [ex.Message]));

            await dbContext.SaveChangesAsync(ct);
        }
    }
}