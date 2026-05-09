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
        IUserCredentialCookieProtector cookieProtector,
        IUserCredentialHandleStore credentialHandleStore,
        CancellationToken ct = default
    )
    {
        var cookie = cookieAccessor.GetCookie();
        var options = new DeliveryOptions();
        var keys = cookieProtector.Unprotect(cookie);

        if (keys is not null)
        {
            var handleResult = await credentialHandleStore.CreateAsync(keys, ct);
            if (handleResult.IsSuccess)
            {
                options.Headers[CredentialsConstants.BackgroundHandleHeaderName] =
                    handleResult.Value;
            }
        }

        await bus.PublishAsync(message, options);
    }
}
