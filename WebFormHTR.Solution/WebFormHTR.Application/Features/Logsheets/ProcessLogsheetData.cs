using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Application.Features.Logsheets.Events;
using Wolverine;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record ProcessLogsheetDataCommand(Guid LogsheetId);

public static class ProcessLogsheetDataHandler
{
    public static async Task Handle(ProcessLogsheetDataCommand request,
        IAppDbContext dbContext,
        IMessageBus bus,
        ILogsheetService logsheetService,
        CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == request.LogsheetId, ct);
        if (logsheet is null)
        {
            await bus.PublishAsync(
                new LogsheetProcessingFinishedEvent(request.LogsheetId, false, "Logsheet not found"));
            await dbContext.SaveChangesAsync(ct);
            return;
        }

        try
        {
            var processResult = await logsheetService.ProcessLogsheetAsync(logsheet, ct);
            string? errorMsg = null;
            if (processResult.IsFailed)
            {
                errorMsg = string.Join(", ", processResult.Errors.Select(e => e.Message));
            }

            await bus.PublishAsync(
                new LogsheetProcessingFinishedEvent(request.LogsheetId, processResult.IsSuccess, errorMsg));

            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            dbContext.ChangeTracker.Clear();
            await bus.PublishAsync(
                new LogsheetProcessingFinishedEvent(request.LogsheetId, false,
                    $"Exception during processing: {ex.Message}"));

            await dbContext.SaveChangesAsync(ct);
        }
    }
}