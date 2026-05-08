using LogsheetXtractor.Application.Interfaces;
using Wolverine;

namespace LogsheetXtractor.Application.Extensions;

public static class MessageBusExtensions
{
    public static ValueTask PublishWithContextAsync<T>(this IMessageBus bus, T message,
        ICredentialCookieAccessor cookieAccessor)
    {
        var cookie = cookieAccessor.GetCookie();

        var options = new DeliveryOptions();
        if (!string.IsNullOrEmpty(cookie))
        {
            options.Headers["UserCookie"] = cookie;
        }

        return bus.PublishAsync(message, options);
    }
}