using System.Text.Json.Serialization;
using WebFormHTR.Application;
using Wolverine;
using Wolverine.Http;
using Mapster;
using WebFormHTR.Infrastructure.Installers;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.API.Middleware;
using WebFormHTR.API.Notifications;
using WebFormHTR.Application.Interfaces;
using Wolverine.FluentValidation;
using Wolverine.EntityFrameworkCore;
using Wolverine.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(ApplicationAssemblyReference).Assembly);
    opts.UseFluentValidation();

    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.UseDurableLocalQueues();
    opts.Durability.Mode = DurabilityMode.Solo;
    opts.PersistMessagesWithSqlite(builder.Configuration.GetConnectionString("DefaultConnection")!);
});
builder.Services.AddWolverineHttp();
builder.Services.AddMapster();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
TypeAdapterConfig.GlobalSettings.Scan(typeof(ApplicationAssemblyReference).Assembly);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, WebSocketNotificationService>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapWolverineEndpoints(x => x.WarmUpRoutes = RouteWarmup.Eager);
app.MapHub<WebFormHTR.API.Hubs.LogsheetHub>("/hubs/logsheets");

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