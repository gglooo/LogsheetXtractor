using LogsheetXtractor.Application.MessageProcessing;
using Wolverine;
using Wolverine.ErrorHandling;

namespace LogsheetXtractor.API.Extensions;

public static class WolverineRetryPolicyExtensions
{
    public static void ApplyRetryPolicy(this WolverineOptions options, MessageRetryPolicy retryPolicy)
    {
        foreach (var exceptionType in retryPolicy.RetryableExceptionTypes)
        {
            options
                .OnExceptionOfType(exceptionType)
                .RetryWithCooldown(retryPolicy.RetryCooldowns.ToArray());
        }
    }
}
