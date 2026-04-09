namespace LogsheetXtractor.Application.MessageProcessing;

public sealed record MessageRetryPolicy(
    IReadOnlyList<Type> RetryableExceptionTypes,
    IReadOnlyList<TimeSpan> RetryCooldowns
)
{
    public int MaxAttempts => RetryCooldowns.Count + 1;

    public bool IsRetryable(Exception exception)
    {
        return Expand(exception).Any(candidate =>
            RetryableExceptionTypes.Any(exceptionType =>
                exceptionType.IsAssignableFrom(candidate.GetType())
            )
        );
    }

    private static IEnumerable<Exception> Expand(Exception exception)
    {
        while (true)
        {
            yield return exception;

            if (exception is AggregateException aggregateException)
            {
                foreach (var inner in aggregateException.Flatten().InnerExceptions)
                {
                    foreach (var nested in Expand(inner))
                    {
                        yield return nested;
                    }
                }

                yield break;
            }

            if (exception.InnerException is null)
            {
                yield break;
            }

            exception = exception.InnerException;
        }
    }
}
