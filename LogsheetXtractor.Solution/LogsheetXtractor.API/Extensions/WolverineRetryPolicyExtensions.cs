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

    public static void ApplyRetryPolicies(
        this WolverineOptions options,
        IEnumerable<MessageRetryPolicy> retryPolicies
    )
    {
        var signatures = new HashSet<string>(StringComparer.Ordinal);

        foreach (var policy in retryPolicies)
        {
            var delaysSignature = string.Join(
                ",",
                policy.RetryCooldowns.Select(delay => delay.Ticks.ToString())
            );

            foreach (var exceptionType in policy.RetryableExceptionTypes)
            {
                var key = $"{exceptionType.FullName}|{delaysSignature}";
                if (!signatures.Add(key))
                {
                    continue;
                }

                options.OnExceptionOfType(exceptionType).RetryWithCooldown(policy.RetryCooldowns.ToArray());
            }
        }
    }
}
