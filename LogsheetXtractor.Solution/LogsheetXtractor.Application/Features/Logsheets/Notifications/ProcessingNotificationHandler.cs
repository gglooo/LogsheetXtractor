using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Logsheets.Notifications;

public static class ProcessingNotificationHandler
{
    public static async Task Handle(
        LogsheetProcessingFinishedEvent message,
        INotificationService notificationService,
        CancellationToken ct
    )
    {
        await notificationService.NotifyLogsheetProcessingFinishedAsync(message, ct);
    }
}
