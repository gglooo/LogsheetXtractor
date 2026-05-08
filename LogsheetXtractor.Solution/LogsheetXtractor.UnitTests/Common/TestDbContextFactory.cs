using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.UnitTests.Common;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var softDeleteInterceptor = new SoftDeleteInterceptor();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(softDeleteInterceptor)
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}
