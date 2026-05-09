using FluentResults;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Credentials;

public sealed record DeleteUserCredentialsCommand(string? Handle);

public static class DeleteUserCredentialsHandler
{
    public static async Task<Result> Handle(
        DeleteUserCredentialsCommand command,
        IUserCredentialHandleStore credentialHandleStore,
        IAppDbContext dbContext,
        CancellationToken ct
    )
    {
        await credentialHandleStore.ReleaseAsync(command.Handle, ct);

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
