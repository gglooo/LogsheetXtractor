namespace WebFormHTR.Application.Interfaces;

public interface IScriptExecutor
{
    Task<string> ExecuteScriptAsync(string scriptName, string jsonPayload, CancellationToken cancellationToken);
}