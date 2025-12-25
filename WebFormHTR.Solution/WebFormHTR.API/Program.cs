using System.Text.Json.Serialization;
using WebFormHTR.Application;
using Wolverine;
using Wolverine.Http;
using Mapster;
using WebFormHTR.Infrastructure.Installers;

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

app.Run();