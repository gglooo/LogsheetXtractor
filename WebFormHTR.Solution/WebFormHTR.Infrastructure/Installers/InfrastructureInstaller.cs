using System.Reflection;
using Docnet.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebFormHTR.Application.Features.ExtractedValues;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.Export;
using WebFormHTR.Application.Features.PdfCropper;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Infrastructure.Persistence.Interceptors;
using WebFormHTR.Infrastructure.Services;
using WebFormHTR.Infrastructure.Services.Coordinates;
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

        services.AddSingleton<IDocLib>(_ => DocLib.Instance);

        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRoiService, RoiService>();
        services.AddScoped<IHtrScriptEngine, PythonHtrAdapter>();
        services.AddScoped<IScriptExecutor, PythonScriptExecutor>();
        services.AddScoped<IOcrCredentialService, OcrCredentialService>();
        services.AddScoped<ICredentialService, CredentialService>();
        services.AddScoped<ICredentialContextProvider, CredentialContextProvider>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IResidualService, ResidualService>();
        services.AddScoped<ILogsheetService, LogsheetService>();
        services.AddScoped<IPdfCropperService, PdfCropperService>();
        services.AddScoped<IScriptOutputParser, ScriptOutputParser>();
        services.AddScoped<IScriptInputPreparer, ScriptInputPreparer>();
        services.AddScoped<ICoordinateTransformerService, CoordinateTransformerService>();
        services.AddScoped<IPerspectiveMatrixComputer, PerspectiveMatrixComputer>();
        services.AddScoped<ILogsheetExportService, LogsheetExportService>();
        services.AddScoped<IExtractedValuesService, ExtractedValuesService>();
        services.AddScoped<IPdfQrCodeScanner, PdfQrCodeScanner>();
    }
}