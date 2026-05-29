using LogsheetXtractor.Application.Features.Credentials;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class UserCredentialContext(
    IEnumerable<(ECredentialType, string)> temporaryCredentialPaths,
    ITemporaryCredentialFileStore temporaryCredentialFileStore,
    ILogger<UserCredentialContext> logger
) : ICredentialContext
{
    public IEnumerable<(ECredentialType, string)> CredentialPaths { get; } =
        temporaryCredentialPaths;

    public ValueTask DisposeAsync()
    {
        foreach (var (_, path) in CredentialPaths)
        {
            try
            {
                if (temporaryCredentialFileStore.Delete(path))
                {
                    logger.LogInformation(
                        "Successfully deleted a temporary personal credential file."
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to delete a temporary personal credential file."
                );
            }
        }

        return ValueTask.CompletedTask;
    }
}
