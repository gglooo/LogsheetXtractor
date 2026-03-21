using LogsheetXtractor.Application.Features.Logsheets.Events;

namespace LogsheetXtractor.Application.Interfaces;

public interface INotificationService
{
    Task NotifyLogsheetProcessingFinishedAsync(
        LogsheetProcessingFinishedEvent notificationEvent,
        CancellationToken ct
    );

    Task NotifyLogsheetAutomaticallyAlignedAsync(
        LogsheetAutomaticallyAlignedEvent notificationEvent,
        CancellationToken ct
    );
}
