using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebFormHTR.Application.Features.Logsheets.Create.Events;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;
using Wolverine;

namespace WebFormHTR.Application.Features.Logsheets.Create;

public static class HandleLogsheetCreatedHandler
{
    public static async Task Handle(
        LogsheetCreatedEvent message,
        IAppDbContext dbContext,
        ILogger<LogsheetCreatedEvent> logger,
        IMessageBus bus,
        CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == message.LogsheetId, ct);
        if (logsheet is null)
        {
            logger.LogInformation(
                "Skipping automatic alignment for Logsheet {LogsheetId}: row not found or not in Pending state",
                message.LogsheetId);
            return;
        }

        if (!message.PerformAutomaticAlignment)
        {
            logger.LogInformation(
                "Skipping automatic alignment for Logsheet {LogsheetId}: disabled by upload settings",
                message.LogsheetId);
            return;
        }

        if (logsheet.Status != ELogSheetStatus.Pending)
        {
            logger.LogInformation(
                "Skipping automatic alignment for Logsheet {LogsheetId}: status is {Status}",
                message.LogsheetId, logsheet.Status);
            return;
        }

        logsheet.Status = ELogSheetStatus.Aligning;
        await bus.PublishAsync(new AlignLogsheetCommand(logsheet.Id));

        await dbContext.SaveChangesAsync(ct);
    }
}
