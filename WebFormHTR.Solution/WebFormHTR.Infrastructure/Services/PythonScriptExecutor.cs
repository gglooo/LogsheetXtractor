using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Infrastructure.Services;

public class PythonScriptExecutor(IConfiguration config) : IScriptExecutor
{
    private readonly string _pythonInterpreterPath = config["Python:InterpreterPath"] ?? "python3";
    private readonly string _scriptsBasePath = config["Python:ScriptsFolder"] ?? "../../../formHTR";

    public async Task<string> ExecuteScriptAsync(string scriptName, string jsonPayload,
        CancellationToken cancellationToken)
    {
        var scriptPath = Path.Combine(_scriptsBasePath, scriptName);
        var formattedArgs = $"\"{scriptPath}\" \"{jsonPayload.Replace("\"", "\\\"")}\"";

        ProcessStartInfo startInfo = new()
        {
            FileName = _pythonInterpreterPath,
            Arguments = formattedArgs,
            UseShellExecute = false,
            // TODO: change, leaving this for debug purposes
            CreateNoWindow = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);

        if (process is null)
        {
            throw new InvalidOperationException("Failed to start the Python script process.");
        }

        using var reader = process.StandardOutput;

        var result = await reader.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Python script execution failed with exit code {process.ExitCode}: {error}");
        }

        return result;
    }
}