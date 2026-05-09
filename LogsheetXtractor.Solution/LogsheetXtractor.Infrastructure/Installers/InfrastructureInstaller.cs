using System.Reflection;
using Docnet.Core;
using LogsheetXtractor.Application.Features.ExtractedValues;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.Export;
using LogsheetXtractor.Application.Features.PdfCropper;
using LogsheetXtractor.Application.Features.Residuals;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Infrastructure.Persistence.Interceptors;
using LogsheetXtractor.Infrastructure.Services;
using LogsheetXtractor.Infrastructure.Services.Coordinates;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using LogsheetXtractor.Infrastructure.Services.Scripting;
using LogsheetXtractor.Infrastructure.Services.Storage;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogsheetXtractor.Infrastructure.Installers;

public static class InfrastructureInstaller
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        var storagePath = config.GetValue<string>("Storage:LocalStoragePath");

        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        services.Configure<UserCredentialCookieOptions>(
            config.GetSection(UserCredentialCookieOptions.SectionName)
        );
        services.Configure<UserCredentialBackgroundSnapshotOptions>(
            config.GetSection(UserCredentialBackgroundSnapshotOptions.SectionName)
        );
        services.PostConfigure<UserCredentialCookieOptions>(options =>
        {
            if (options.Ttl <= TimeSpan.Zero)
            {
                options.Ttl = TimeSpan.FromDays(365);
            }
        });
        services.PostConfigure<UserCredentialBackgroundSnapshotOptions>(options =>
        {
            if (options.Ttl <= TimeSpan.Zero)
            {
                options.Ttl = TimeSpan.FromDays(7);
            }
        });

        var dataProtectionBuilder = services.AddDataProtection()
            .SetApplicationName("LogsheetXtractor");
        if (!string.IsNullOrWhiteSpace(storagePath))
        {
            var keyDirectory = new DirectoryInfo(Path.Combine(storagePath, "keys"));
            keyDirectory.Create();
            dataProtectionBuilder.PersistKeysToFileSystem(keyDirectory);
        }

        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddDbContext<AppDbContext>(
            (sp, options) =>
                options
                    .UseSqlite(connectionString)
                    .UseLazyLoadingProxies()
                    .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>())
        );

        services.AddScoped<IAppDbContext, AppDbContext>();

        services.AddSingleton<IDocLib>(_ => DocLib.Instance);

        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRoiService, RoiService>();
        services.AddScoped<IHtrScriptEngine, PythonHtrAdapter>();
        services.AddScoped<IScriptExecutor, PythonScriptExecutor>();
        services.AddScoped<IOcrCredentialService, OcrCredentialService>();
        services.AddScoped<ICredentialService, CredentialService>();
        services.AddScoped<ICredentialContextProvider, CredentialContextProvider>();
        services.AddSingleton<ITemporaryCredentialFileStore, TemporaryCredentialFileStore>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IResidualService, ResidualService>();
        services.AddScoped<ILogsheetService, LogsheetService>();
        services.AddScoped<IPdfCropperService, PdfCropperService>();
        services.AddScoped<IScriptOutputParser, ScriptOutputParser>();
        services.AddScoped<IPythonScriptArgumentsBuilder, PythonScriptArgumentsBuilder>();
        services.AddScoped<IScriptInputPreparer, ScriptInputPreparer>();
        services.AddScoped<ICoordinateTransformerService, CoordinateTransformerService>();
        services.AddScoped<IPerspectiveMatrixComputer, PerspectiveMatrixComputer>();
        services.AddScoped<ILogsheetExportService, LogsheetExportService>();
        services.AddScoped<IExtractedValuesService, ExtractedValuesService>();
        services.AddScoped<IPdfQrCodeScanner, PdfQrCodeScanner>();
        services.AddScoped<ICredentialCookieAccessor, CredentialCookieAccessor>();
        services.AddSingleton<IUserCredentialCookieProtector, DataProtectionUserCredentialCookieProtector>();
        services.AddSingleton<IUserCredentialSnapshotProtector, DataProtectionUserCredentialSnapshotProtector>();

        services.AddRoiValidation();

        services.AddMemoryCache();
    }
}
