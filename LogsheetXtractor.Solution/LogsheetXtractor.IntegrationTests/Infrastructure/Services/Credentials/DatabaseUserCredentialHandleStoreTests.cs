using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public sealed class DatabaseUserCredentialHandleStoreTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ServiceProvider _serviceProvider;
    private readonly List<IServiceScope> _scopes = [];

    public DatabaseUserCredentialHandleStoreTests()
    {
        _connection.Open();

        _serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(options => options.UseSqlite(_connection))
            .BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateAndResolveAsync_ShouldStoreEncryptedCredentialsBehindOpaqueHandle()
    {
        var store = CreateStore(TimeSpan.FromDays(7));
        var credentials = new Dictionary<ECredentialType, string>
        {
            [ECredentialType.Google] = " google-key ",
            [ECredentialType.Azure] = "azure-key",
            [ECredentialType.Amazon] = "   ",
        };

        var createResult = await store.CreateAsync(credentials);

        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Should().HaveLength(32);

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var storedHandle = await dbContext.UserCredentialHandles.SingleAsync();
            storedHandle.Handle.Should().Be(createResult.Value);
            storedHandle.ProtectedPayload.Should().NotContain("google-key");
            storedHandle.ProtectedPayload.Should().NotContain("azure-key");
        }

        var resolveResult = await store.ResolveAsync(createResult.Value);

        resolveResult.IsSuccess.Should().BeTrue();
        resolveResult.Value.Should().BeEquivalentTo(
            new Dictionary<ECredentialType, string>
            {
                [ECredentialType.Google] = "google-key",
                [ECredentialType.Azure] = "azure-key",
            }
        );
    }

    [Fact]
    public async Task ResolveAsync_ShouldRejectTamperedPayload()
    {
        var store = CreateStore(TimeSpan.FromDays(7));
        var createResult = await store.CreateAsync(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "google-key" }
        );

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var storedHandle = await dbContext.UserCredentialHandles.SingleAsync();
            storedHandle.ProtectedPayload = "tampered";
            await dbContext.SaveChangesAsync();
        }

        var resolveResult = await store.ResolveAsync(createResult.Value);

        resolveResult.IsFailed.Should().BeTrue();
        resolveResult.Errors.Should().ContainSingle(e =>
            e.Message == CredentialsConstants.ExpiredBackgroundCredentialHandleMessage
        );
        using var verifyScope = _serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyContext.UserCredentialHandles.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ResolveAsync_ShouldRejectAndDeleteExpiredHandle()
    {
        var store = CreateStore(TimeSpan.FromMilliseconds(10));
        var createResult = await store.CreateAsync(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "google-key" }
        );

        await Task.Delay(TimeSpan.FromMilliseconds(50));

        var resolveResult = await store.ResolveAsync(createResult.Value);

        resolveResult.IsFailed.Should().BeTrue();
        resolveResult.Errors.Should().ContainSingle(e =>
            e.Message == CredentialsConstants.ExpiredBackgroundCredentialHandleMessage
        );
        using var verifyScope = _serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyContext.UserCredentialHandles.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ReleaseAsync_ShouldDeleteHandle()
    {
        var store = CreateStore(TimeSpan.FromDays(7));
        var createResult = await store.CreateAsync(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "google-key" }
        );

        await store.ReleaseAsync(createResult.Value);

        using var verifyScope = _serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyContext.UserCredentialHandles.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CleanupExpiredAsync_ShouldDeleteOnlyExpiredHandles()
    {
        var expiredStore = CreateStore(TimeSpan.FromMilliseconds(10));
        await expiredStore.CreateAsync(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "old-key" }
        );
        await Task.Delay(TimeSpan.FromMilliseconds(50));

        var freshStore = CreateStore(TimeSpan.FromDays(7));
        await freshStore.CreateAsync(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "fresh-key" }
        );

        var deletedCount = await freshStore.CleanupExpiredAsync();

        deletedCount.Should().Be(1);
        using var verifyScope = _serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var remainingHandle = await verifyContext.UserCredentialHandles.SingleAsync();
        remainingHandle.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    public void Dispose()
    {
        foreach (var scope in _scopes)
        {
            scope.Dispose();
        }

        _serviceProvider.Dispose();
        _connection.Dispose();
    }

    private DatabaseUserCredentialHandleStore CreateStore(TimeSpan ttl)
    {
        var provider = DataProtectionProvider.Create("DatabaseUserCredentialHandleStoreTests");
        var scope = _serviceProvider.CreateScope();
        _scopes.Add(scope);

        return new DatabaseUserCredentialHandleStore(
            provider,
            scope.ServiceProvider.GetRequiredService<AppDbContext>(),
            Options.Create(new UserCredentialBackgroundHandleOptions { Ttl = ttl }),
            NullLogger<DatabaseUserCredentialHandleStore>.Instance
        );
    }
}
