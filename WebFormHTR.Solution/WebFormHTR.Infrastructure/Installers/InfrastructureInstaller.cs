using System.Reflection;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebFormHTR.Application;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Infrastructure.Persistence.Interceptors;
using WebFormHTR.Infrastructure.Services;
using WebFormHTR.Infrastructure.Services.Credentials;
using WebFormHTR.Infrastructure.Services.Scripting;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Installers;

public static class InfrastructureInstaller
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseSqlite(connectionString)
            .UseLazyLoadingProxies()
            .AddInterceptors(
                sp.GetRequiredService<SoftDeleteInterceptor>()));

        services.AddScoped<IAppDbContext, AppDbContext>();

        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRoiService, RoiService>();
        services.AddScoped<IHtrScriptEngine, PythonHtrAdapter>();
        services.AddScoped<IScriptExecutor, PythonScriptExecutor>();
        services.AddScoped<ICredentialService, CredentialService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IResidualService, ResidualService>();
        services.AddScoped<ILogsheetService, LogsheetService>();
    }
}