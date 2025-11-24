using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Infrastructure.Persistence.Interceptors;
using WebFormHTR.Infrastructure.Services;

namespace WebFormHTR.Infrastructure.Installers;

public static class InfrastructureInstaller
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddDbContext<AppDbContext>(
            (sp, options) => options
                // TODO: allow user to configure
                .UseSqlite("Data Source=app.db")
                .UseLazyLoadingProxies()
                .AddInterceptors(
                    sp.GetRequiredService<SoftDeleteInterceptor>()));
        
        services.AddScoped<IAppDbContext, AppDbContext>();

        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRoiService, RoiService>();
    }
}