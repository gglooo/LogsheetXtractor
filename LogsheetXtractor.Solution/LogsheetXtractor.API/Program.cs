using System.Text.Json.Serialization;
using LogsheetXtractor.API.Middleware;
using LogsheetXtractor.API.Notifications;
using LogsheetXtractor.Application;
using LogsheetXtractor.Application.Installers;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Installers;
using LogsheetXtractor.Infrastructure.Middleware;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var isTestingEnvironment = builder.Environment.IsEnvironment("Testing");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFileName = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

    if (File.Exists(xmlFilePath))
    {
        options.IncludeXmlComments(xmlFilePath);
    }
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(ApplicationAssemblyReference).Assembly);
    opts.UseFluentValidation();

    opts.UseEntityFrameworkCoreTransactions();
    opts.Durability.Mode = DurabilityMode.Solo;
    if (!isTestingEnvironment)
    {
        opts.Policies.UseDurableLocalQueues();
        opts.PersistMessagesWithSqlite(builder.Configuration.GetConnectionString("DefaultConnection")!);
    }
    
    opts.Policies.AddMiddleware(typeof(CredentialCookieMiddleware));
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
app.MapHub<LogsheetXtractor.API.Hubs.LogsheetHub>("/hubs/logsheets");

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

        var context =
            services.GetRequiredService<LogsheetXtractor.Infrastructure.Persistence.AppDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during startup initialization.");
        throw;
    }
}

app.Run();

public partial class Program;
