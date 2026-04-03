using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.E2ETests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.E2ETests.Credentials;

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

    [Fact]
    public async Task SetCredentialsAndGetStatus_ShouldReflectUserCredentialsFromCookie()
    {
        var setResponse = await _client.PostAsJsonAsync(
            "/api/credentials",
            new
            {
                keys = new Dictionary<string, string>
                {
                    ["Google"] = "google-key",
                    ["Azure"] = "azure-key",
                },
            }
        );
        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cookieHeader = GetCookieHeaderValue(setResponse, CredentialsConstants.CookieName);

        using var statusRequest = new HttpRequestMessage(HttpMethod.Get, "/api/credentials/status");
        statusRequest.Headers.Add("Cookie", cookieHeader);
        var statusResponse = await _client.SendAsync(statusRequest);

        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusPayload = await statusResponse.Content.ReadFromJsonAsync<CredentialsStatusResponse>();
        statusPayload.Should().NotBeNull();
        statusPayload!.HasUserCredentials.Should().BeTrue();
        statusPayload.Available.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCredentials_ShouldClearCookieAndStatusShouldNotHaveUserCredentials()
    {
        var setResponse = await _client.PostAsJsonAsync(
            "/api/credentials",
            new
            {
                keys = new Dictionary<string, string> { ["Google"] = "google-key" },
            }
        );
        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookieHeader = GetCookieHeaderValue(setResponse, CredentialsConstants.CookieName);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/credentials");
        deleteRequest.Headers.Add("Cookie", cookieHeader);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var clearedCookieHeader = GetCookieHeaderValue(deleteResponse, CredentialsConstants.CookieName);
        using var statusRequest = new HttpRequestMessage(HttpMethod.Get, "/api/credentials/status");
        statusRequest.Headers.Add("Cookie", clearedCookieHeader);
        var statusResponse = await _client.SendAsync(statusRequest);

        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusPayload = await statusResponse.Content.ReadFromJsonAsync<CredentialsStatusResponse>();
        statusPayload.Should().NotBeNull();
        statusPayload!.HasUserCredentials.Should().BeFalse();
    }

    private static string GetCookieHeaderValue(HttpResponseMessage response, string cookieName)
    {
        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).Should().BeTrue();
        var cookie = setCookieHeaders!
            .FirstOrDefault(header => header.StartsWith($"{cookieName}=", StringComparison.Ordinal));
        cookie.Should().NotBeNull();

        return cookie!.Split(';', 2)[0];
    }

    private sealed record CredentialsStatusResponse(bool Available, bool HasUserCredentials);
}
