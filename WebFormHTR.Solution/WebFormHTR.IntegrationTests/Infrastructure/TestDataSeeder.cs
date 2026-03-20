using Microsoft.Extensions.DependencyInjection;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using File = WebFormHTR.Domain.Entities.File;

namespace WebFormHTR.IntegrationTests.Infrastructure;

public static class TestDataSeeder
{
    public static async Task<Guid> SeedEditableTemplateAsync(ApiWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var file = new File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "./app_data",
            ContentType = "application/pdf",
            SizeBytes = 100,
        };

        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = $"Template-{Guid.NewGuid():N}",
            FileId = file.Id,
            File = file,
            Width = 1000,
            Height = 1400,
        };

        dbContext.Files.Add(file);
        dbContext.Templates.Add(template);
        await dbContext.SaveChangesAsync();

        return template.Id;
    }
}
