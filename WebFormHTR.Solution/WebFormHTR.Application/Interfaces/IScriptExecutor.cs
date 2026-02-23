namespace WebFormHTR.Application.Interfaces;

public interface IScriptExecutor
{
    Task<string> ExecuteScriptAsync(string scriptName, IEnumerable<string> args, CancellationToken cancellationToken);
    Task<T> ExecuteScriptWithJsonOutputAsync<T>(string scriptName, IEnumerable<string> args, CancellationToken cancellationToken);
}