using FluentAssertions;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Features.Logsheets.Notifications;
using LogsheetXtractor.Application.Interfaces;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets.Notifications;

public class NotificationHandlerTests
{
    [Fact]
    public async Task ProcessingNotificationHandler_ShouldForwardProcessingFinishedEvent()
    {
        var notificationServiceMock = new Mock<INotificationService>();
        var notificationEvent = new LogsheetProcessingFinishedEvent(
            Guid.NewGuid(),
            IsSuccess: true,
            ErrorMessage: null
        );

        await ProcessingNotificationHandler.Handle(
            notificationEvent,
            notificationServiceMock.Object,
            CancellationToken.None
        );

        notificationServiceMock.Verify(
            service => service.NotifyLogsheetProcessingFinishedAsync(notificationEvent, CancellationToken.None),
            Times.Once
        );
    }

    [Fact]
    public async Task AlignmentNotificationHandler_ShouldForwardAlignmentFinishedEvent()
    {
        var notificationServiceMock = new Mock<INotificationService>();
        var notificationEvent = new LogsheetAutomaticAlignmentFinished(
            Guid.NewGuid(),
            IsSuccess: false,
            ErrorMessage: "alignment failed"
        );

        await AlignmentNotificationHandler.Handle(
            notificationEvent,
            notificationServiceMock.Object,
            CancellationToken.None
        );

        notificationServiceMock.Verify(
            service => service.NotifyLogsheetAutomaticallyAlignedAsync(notificationEvent, CancellationToken.None),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessingNotificationHandler_ShouldSurfaceNotificationFailureWithoutPersistenceWork()
    {
        var notificationServiceMock = new Mock<INotificationService>();
        var notificationEvent = new LogsheetProcessingFinishedEvent(
            Guid.NewGuid(),
            IsSuccess: false,
            ErrorMessage: "processing failed"
        );
        notificationServiceMock
            .Setup(service =>
                service.NotifyLogsheetProcessingFinishedAsync(notificationEvent, CancellationToken.None)
            )
            .ThrowsAsync(new InvalidOperationException("SignalR unavailable"));

        var act = async () =>
            await ProcessingNotificationHandler.Handle(
                notificationEvent,
                notificationServiceMock.Object,
                CancellationToken.None
            );

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("SignalR unavailable");
    }
}
