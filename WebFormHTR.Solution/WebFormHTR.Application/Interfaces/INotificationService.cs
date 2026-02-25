using WebFormHTR.Application.Features.Logsheets.Events;

namespace WebFormHTR.Application.Interfaces;

public interface INotificationService
{
    Task NotifyLogsheetProcessingFinishedAsync(LogsheetProcessingFinishedEvent notificationEvent, CancellationToken ct);
    Task NotifyBatchProcessingFinishedAsync(BatchProcessingFinishedEvent notificationEvent, CancellationToken ct);
}
