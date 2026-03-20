using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WebFormHTR.IntegrationTests.Infrastructure;

namespace WebFormHTR.IntegrationTests.Logsheets;

public class LogsheetEndpointsIntegrationSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LogsheetEndpointsIntegrationSmokeTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task GetLogsheet_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/logsheets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetProofreading_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        var response = await _client.PostAsync(
            $"/api/logsheets/{Guid.NewGuid()}/proofreading/reset",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
