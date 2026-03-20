using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.IntegrationTests.Infrastructure.Fakes;

namespace WebFormHTR.IntegrationTests.Infrastructure;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _tempRootPath = Path.Combine(
        Path.GetTempPath(),
        $"webformhtr-integration-{Guid.NewGuid():N}");

    private readonly string _storagePath;
    private readonly string _dbPath;

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

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["Storage:LocalStoragePath"] = _storagePath,
            });
        });

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
