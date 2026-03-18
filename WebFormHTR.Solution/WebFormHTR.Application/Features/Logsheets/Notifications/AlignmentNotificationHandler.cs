using WebFormHTR.Application.Features.Logsheets.Events;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets.Notifications;

public static class AlignmentNotificationHandler
{
    public static async Task Handle(
        LogsheetAutomaticallyAlignedEvent message,
        INotificationService notificationService,
        CancellationToken ct)
    {
        await notificationService.NotifyLogsheetAutomaticallyAlignedAsync(message, ct);
    }
}