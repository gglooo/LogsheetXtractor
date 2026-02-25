using WebFormHTR.Application.Features.Logsheets.Events;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public static class ProcessingNotificationHandler
{
    public static async Task Handle(
        LogsheetProcessingFinishedEvent message,
        INotificationService notificationService,
        CancellationToken ct)
    {
        await notificationService.NotifyLogsheetProcessingFinishedAsync(message, ct);
    }

    public static async Task Handle(
        BatchProcessingFinishedEvent message,
        INotificationService notificationService,
        CancellationToken ct)
    {
        await notificationService.NotifyBatchProcessingFinishedAsync(message, ct);
    }
}