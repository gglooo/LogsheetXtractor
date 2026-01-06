using System.Text.Json.Serialization;
using WebFormHTR.Application;
using Wolverine;
using Wolverine.Http;
using Mapster;
using WebFormHTR.Infrastructure.Installers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Host.UseWolverine(opts => { opts.Discovery.IncludeAssembly(typeof(ApplicationAssemblyReference).Assembly); });
builder.Services.AddWolverineHttp();
builder.Services.AddMapster();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
TypeAdapterConfig.GlobalSettings.Scan(typeof(ApplicationAssemblyReference).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapWolverineEndpoints(x => x.WarmUpRoutes = RouteWarmup.Eager);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var storagePath = config.GetValue<string>("Storage:LocalStoragePath");
        if (!string.IsNullOrEmpty(storagePath) && !Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
            logger.LogInformation("Created storage directory: {Path}", storagePath);
        }

        var context = services.GetRequiredService<WebFormHTR.Infrastructure.Persistence.AppDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during startup initialization.");
    }
}

app.Run();