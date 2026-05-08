using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.E2ETests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.E2ETests.Credentials;

public class CredentialsEndpointsIntegrationSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CredentialsEndpointsIntegrationSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
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
        var cookieValue = Uri.UnescapeDataString(cookieHeader.Split('=', 2)[1]);
        cookieValue.Should().StartWith("v1:");
        cookieValue.Should().NotContain("google-key");
        cookieValue.Should().NotContain("azure-key");

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
    public async Task SetCredentials_ShouldCreateProtectedApiCookieWithOneYearLifetime()
    {
        var beforeRequest = DateTimeOffset.UtcNow;
        var setResponse = await _client.PostAsJsonAsync(
            "/api/credentials",
            new
            {
                keys = new Dictionary<string, string> { ["Google"] = "google-key" },
            }
        );
        var afterRequest = DateTimeOffset.UtcNow;

        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var setCookieHeader = GetSetCookieHeader(setResponse, CredentialsConstants.CookieName);
        var normalizedSetCookieHeader = setCookieHeader.ToLowerInvariant();

        normalizedSetCookieHeader.Should().Contain("httponly");
        normalizedSetCookieHeader.Should().Contain("samesite=lax");
        normalizedSetCookieHeader.Should().Contain("secure");
        normalizedSetCookieHeader.Should().Contain("path=/api");
        setCookieHeader.Should().NotContain("google-key");

        var expires = GetCookieExpires(setCookieHeader);
        expires.Should().BeAfter(beforeRequest.AddDays(364));
        expires.Should().BeBefore(afterRequest.AddDays(366));
    }

    [Fact]
    public async Task GetStatus_WithLegacyRawJsonCookie_ShouldIgnoreUserCredentials()
    {
        using var statusRequest = new HttpRequestMessage(HttpMethod.Get, "/api/credentials/status");
        statusRequest.Headers.Add(
            "Cookie",
            $"{CredentialsConstants.CookieName}={{%22Google%22:%22google-key%22}}"
        );

        var statusResponse = await _client.SendAsync(statusRequest);

        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusPayload = await statusResponse.Content.ReadFromJsonAsync<CredentialsStatusResponse>();
        statusPayload.Should().NotBeNull();
        statusPayload!.HasUserCredentials.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatus_WithTamperedProtectedCookie_ShouldIgnoreUserCredentials()
    {
        using var client = CreateClient(handleCookies: false);
        var setResponse = await client.PostAsJsonAsync(
            "/api/credentials",
            new
            {
                keys = new Dictionary<string, string> { ["Google"] = "google-key" },
            }
        );
        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookieHeader = GetCookieHeaderValue(setResponse, CredentialsConstants.CookieName);
        var cookieValue = Uri.UnescapeDataString(cookieHeader.Split('=', 2)[1]);
        var tamperedCookieValue = cookieValue[..^1] + (cookieValue[^1] == 'A' ? 'B' : 'A');
        var tamperedCookieHeader =
            $"{CredentialsConstants.CookieName}={Uri.EscapeDataString(tamperedCookieValue)}";

        using var statusRequest = new HttpRequestMessage(HttpMethod.Get, "/api/credentials/status");
        statusRequest.Headers.Add("Cookie", tamperedCookieHeader);

        var statusResponse = await client.SendAsync(statusRequest);

        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusPayload = await statusResponse.Content.ReadFromJsonAsync<CredentialsStatusResponse>();
        statusPayload.Should().NotBeNull();
        statusPayload!.HasUserCredentials.Should().BeFalse();
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
        return GetSetCookieHeader(response, cookieName).Split(';', 2)[0];
    }

    private static string GetSetCookieHeader(HttpResponseMessage response, string cookieName)
    {
        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).Should().BeTrue();
        var cookie = setCookieHeaders!
            .FirstOrDefault(header => header.StartsWith($"{cookieName}=", StringComparison.Ordinal));
        cookie.Should().NotBeNull();

        return cookie!;
    }

    private static DateTimeOffset GetCookieExpires(string setCookieHeader)
    {
        var expiresPart = setCookieHeader
            .Split(';', StringSplitOptions.TrimEntries)
            .Single(part => part.StartsWith("expires=", StringComparison.OrdinalIgnoreCase));

        return DateTimeOffset.Parse(expiresPart["expires=".Length..]);
    }

    private HttpClient CreateClient(bool handleCookies)
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = handleCookies,
        });
    }

    private sealed record CredentialsStatusResponse(bool Available, bool HasUserCredentials);
}
