using System.Net;
using FluentAssertions;
using LogsheetXtractor.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.IntegrationTests.Credentials;

public class CredentialsEndpointsIntegrationSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CredentialsEndpointsIntegrationSmokeTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task GetCredentialsStatus_ShouldReturnOkOrNoContent()
    {
        var response = await _client.GetAsync("/api/credentials/status");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }
}
