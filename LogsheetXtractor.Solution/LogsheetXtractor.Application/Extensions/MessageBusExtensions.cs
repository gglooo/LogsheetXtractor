using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Wolverine;

namespace LogsheetXtractor.Application.Extensions;

public static class MessageBusExtensions
{
    public static async ValueTask PublishWithContextAsync<T>(
        this IMessageBus bus,
        T message,
        ICredentialCookieAccessor cookieAccessor,
        CancellationToken ct = default
    )
    {
        var cookie = cookieAccessor.GetCookie();
        var options = new DeliveryOptions();
        if (cookie is not null)
        {
            options.Headers[CredentialsConstants.UserCredentialHandleHeaderName] = cookie;
        }

        await bus.PublishAsync(message, options);
    }
}
