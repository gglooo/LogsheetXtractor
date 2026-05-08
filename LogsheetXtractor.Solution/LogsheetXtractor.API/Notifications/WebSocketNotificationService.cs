using LogsheetXtractor.API.Hubs;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LogsheetXtractor.API.Notifications;

public class WebSocketNotificationService(IHubContext<LogsheetHub> hubContext)
    : INotificationService
{
    public async Task NotifyLogsheetProcessingFinishedAsync(
        LogsheetProcessingFinishedEvent notificationEvent,
        CancellationToken ct
    )
    {
        await hubContext.Clients.All.SendAsync("LogsheetProcessingFinished", notificationEvent, ct);
    }

    public async Task NotifyLogsheetAutomaticallyAlignedAsync(
        LogsheetAutomaticAlignmentFinished notificationEvent,
        CancellationToken ct
    )
    {
        await hubContext.Clients.All.SendAsync(
            "LogsheetAutomaticAlignmentFinished",
            notificationEvent,
            ct
        );
    }
}
