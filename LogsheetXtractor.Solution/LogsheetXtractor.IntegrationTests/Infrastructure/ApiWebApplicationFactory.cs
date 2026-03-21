using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.IntegrationTests.Infrastructure.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading;

namespace LogsheetXtractor.IntegrationTests.Infrastructure;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _tempRootPath = Path.Combine(
        Path.GetTempPath(),
        $"logsheetxtractor-integration-{Guid.NewGuid():N}"
    );

    private readonly string _storagePath;
    private readonly string _dbPath;
    private int _cleanupDone;

    public ApiWebApplicationFactory()
    {
        _storagePath = Path.Combine(_tempRootPath, "storage");
        _dbPath = Path.Combine(_tempRootPath, "integration.sqlite");

        Directory.CreateDirectory(_storagePath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("Storage:LocalStoragePath", _storagePath);

        builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                        ["Storage:LocalStoragePath"] = _storagePath,
                    }
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHtrScriptEngine>();
            services.AddSingleton<IHtrScriptEngine, FakeHtrScriptEngine>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        CleanupTempDirectory();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        CleanupTempDirectory();
    }

    private void CleanupTempDirectory()
    {
        if (Interlocked.Exchange(ref _cleanupDone, 1) == 1)
        {
            return;
        }

        if (!Directory.Exists(_tempRootPath))
        {
            return;
        }

        try
        {
            Directory.Delete(_tempRootPath, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
