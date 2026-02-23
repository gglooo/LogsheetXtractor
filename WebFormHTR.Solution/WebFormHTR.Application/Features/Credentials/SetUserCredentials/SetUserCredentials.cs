using FluentResults;

namespace WebFormHTR.Application.Features.Credentials.SetUserCredentials;

public sealed record SetUserCredentialsCommand(Dictionary<ECredentialType, string> Keys);

public static class SetUserCredentialsHandler
{
    public static async Task<Result> Handle(SetUserCredentialsCommand command, CancellationToken ct)
    {
        return await Task.FromResult(Result.Ok());
    }
}