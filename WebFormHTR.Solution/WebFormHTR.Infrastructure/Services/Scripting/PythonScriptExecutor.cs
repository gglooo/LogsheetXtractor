using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Infrastructure.Services;

public class PythonScriptExecutor(IConfiguration config) : IScriptExecutor
{
    private readonly string _pythonInterpreterPath = config["Python:InterpreterPath"] ?? "python3";
    private readonly string _scriptsBasePath = config["Python:ScriptsFolder"] ?? "../../formHTR";

    public async Task<string> ExecuteScriptAsync(string scriptName, string args,
        CancellationToken cancellationToken)
    {
        var scriptPath = Path.Combine(_scriptsBasePath, scriptName);
        var formattedArgs = $"\"{scriptPath}\" {args.Replace("\"", "\\\"")}";

        Console.WriteLine("Executing Python script:");
        Console.WriteLine($"{_pythonInterpreterPath} {formattedArgs}");

        ProcessStartInfo startInfo = new()
        {
            FileName = _pythonInterpreterPath,
            Arguments = formattedArgs,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var pythonEnvBinPath = Path.GetDirectoryName(_pythonInterpreterPath);

        if (!string.IsNullOrEmpty(pythonEnvBinPath))
        {
            var pathVar =
                System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                    .Windows)
                    ? "Path"
                    : "PATH";

            var currentPath = startInfo.EnvironmentVariables.ContainsKey(pathVar)
                ? startInfo.EnvironmentVariables[pathVar]
                : Environment.GetEnvironmentVariable(pathVar);

            startInfo.EnvironmentVariables[pathVar] = $"{pythonEnvBinPath}{Path.PathSeparator}{currentPath}";
        }

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