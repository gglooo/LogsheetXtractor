using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.IntegrationTests.ExtractedValues;

public class ExtractedValueEndpointsIntegrationSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExtractedValueEndpointsIntegrationSmokeTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task VerifyExtractedValue_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/extracted-values/{Guid.NewGuid()}/verify",
            new
            {
                correctedValue = "123",
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRandomUnverifiedExtractedValue_ShouldReturnNoContent_WhenDatabaseIsEmpty()
    {
        var response = await _client.GetAsync("/api/extracted-values/unverified/random");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
