using LogsheetXtractor.Application.Features.ExtractedValues;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Residuals;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LogsheetXtractor.Application.Installers;

public static class ApplicationInstaller
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IResidualService, ResidualService>();
        services.AddScoped<IRoiService, RoiService>();
        services.AddScoped<ILogsheetService, LogsheetService>();
        services.AddScoped<IExtractedValuesService, ExtractedValuesService>();
        services.AddScoped<ITemplateService, TemplateService>();

        services.AddRoiValidation();

        return services;
    }
}
