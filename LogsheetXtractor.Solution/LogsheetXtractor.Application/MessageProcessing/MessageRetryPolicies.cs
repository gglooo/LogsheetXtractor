using System.Data.Common;
using LogsheetXtractor.Application.Features.Logsheets;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.MessageProcessing;

public static class MessageRetryPolicies
{
    private static readonly IReadOnlyDictionary<Type, MessageRetryPolicy> PoliciesByMessageType =
        new Dictionary<Type, MessageRetryPolicy>
        {
            [typeof(ProcessLogsheetDataCommand)] = new (
                RetryableExceptionTypes:
                [
                    typeof(TimeoutException),
                    typeof(DbException),
                    typeof(DbUpdateException),
                    typeof(DbUpdateConcurrencyException),
                ],
                RetryCooldowns:
                [
                    TimeSpan.FromMilliseconds(50),
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(250),
                ]
            ),
        };

    public static MessageRetryPolicy For<TMessage>()
    {
        return For(typeof(TMessage));
    }

    public static MessageRetryPolicy For(Type messageType)
    {
        if (!PoliciesByMessageType.TryGetValue(messageType, out var policy))
        {
            throw new InvalidOperationException(
                $"No retry policy has been registered for message type '{messageType.FullName}'."
            );
        }

        return policy;
    }
}
