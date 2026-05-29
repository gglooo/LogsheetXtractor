using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Logsheets.Notifications;

public static class AlignmentNotificationHandler
{
    public static async Task Handle(
        LogsheetAutomaticAlignmentFinished message,
        INotificationService notificationService,
        CancellationToken ct
    )
    {
        await notificationService.NotifyLogsheetAutomaticallyAlignedAsync(message, ct);
    }
}
