using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Infrastructure.Services.Storage;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class UserCredentialContext(
    IEnumerable<(ECredentialType, string)> temporaryCredentialPaths,
    IFileStorageService fileStorageService,
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
                if (fileStorageService.DeleteFile(path))
                {
                    logger.LogInformation(
                        "Successfully deleted temporary personal credential file: {Path}",
                        path
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to delete temporary personal credential file: {Path}",
                    path
                );
            }
        }

        return ValueTask.CompletedTask;
    }
}
