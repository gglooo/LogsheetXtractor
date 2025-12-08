namespace WebFormHTR.Application.Interfaces;

public interface IScriptExecutor
{
    Task<string> ExecuteScriptAsync(string scriptName, string args, CancellationToken cancellationToken);
    Task<T> ExecuteScriptWithJsonOutputAsync<T>(string scriptName, string args, CancellationToken cancellationToken);
}