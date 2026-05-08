using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Wolverine;

namespace LogsheetXtractor.Application.Extensions;

public static class MessageBusExtensions
{
    public static ValueTask PublishWithContextAsync<T>(
        this IMessageBus bus,
        T message,
        ICredentialCookieAccessor cookieAccessor,
        IUserCredentialCookieProtector cookieProtector,
        IUserCredentialSnapshotProtector snapshotProtector
    )
    {
        var cookie = cookieAccessor.GetCookie();
        var options = new DeliveryOptions();
        var keys = cookieProtector.Unprotect(cookie);

        if (keys is not null)
        {
            options.Headers[CredentialsConstants.BackgroundSnapshotHeaderName] =
                snapshotProtector.Protect(keys);
        }

        return bus.PublishAsync(message, options);
    }
}
