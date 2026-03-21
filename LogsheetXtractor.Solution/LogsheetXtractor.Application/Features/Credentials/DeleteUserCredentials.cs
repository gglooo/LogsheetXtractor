using FluentResults;

namespace LogsheetXtractor.Application.Features.Credentials;

public sealed record DeleteUserCredentialsCommand;

public static class DeleteUserCredentialsHandler
{
    public static async Task<Result> Handle(DeleteUserCredentialsCommand command, CancellationToken ct)
    {
        return Result.Ok();
    }
}