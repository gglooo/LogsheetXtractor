using WebFormHTR.Application;
using WebFormHTR.Infrastructure.Persistence.Installers;
using Wolverine;
using Wolverine.Http;
using Mapster;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure();
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(ApplicationAssemblyReference).Assembly);
});
builder.Services.AddWolverineHttp();
builder.Services.AddMapster();
TypeAdapterConfig.GlobalSettings.Scan(typeof(ApplicationAssemblyReference).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapWolverineEndpoints();

app.Run();

