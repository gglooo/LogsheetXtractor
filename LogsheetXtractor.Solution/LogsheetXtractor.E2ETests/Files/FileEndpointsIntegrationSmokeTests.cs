using System.Net;
using FluentAssertions;
using LogsheetXtractor.E2ETests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.E2ETests.Files;

public class FileEndpointsIntegrationSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FileEndpointsIntegrationSmokeTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task GetFile_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/files/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFileImage_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/files/{Guid.NewGuid()}/image");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
