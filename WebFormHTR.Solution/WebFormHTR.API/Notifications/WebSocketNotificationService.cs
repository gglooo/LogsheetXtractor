using Microsoft.AspNetCore.SignalR;
using WebFormHTR.API.Hubs;
using WebFormHTR.Application.Features.Logsheets.Events;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.API.Notifications;

public class WebSocketNotificationService(IHubContext<LogsheetHub> hubContext) : INotificationService
{
    public async Task NotifyLogsheetProcessingFinishedAsync(LogsheetProcessingFinishedEvent notificationEvent,
        CancellationToken ct)
    {
        await hubContext.Clients.All.SendAsync("LogsheetProcessingFinished", notificationEvent, ct);
    }

    public async Task NotifyLogsheetAutomaticallyAlignedAsync(LogsheetAutomaticallyAlignedEvent notificationEvent,
        CancellationToken ct)
    {
        await hubContext.Clients.All.SendAsync("LogsheetAutomaticAlignmentFinished", notificationEvent, ct);
    }
}