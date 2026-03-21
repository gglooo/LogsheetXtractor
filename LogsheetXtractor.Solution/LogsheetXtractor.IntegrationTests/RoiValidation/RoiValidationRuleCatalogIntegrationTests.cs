using System.Net;
using System.Text.Json;
using FluentAssertions;
using LogsheetXtractor.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.IntegrationTests.RoiValidation;

public class RoiValidationRuleCatalogIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoiValidationRuleCatalogIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task GetRuleCatalog_ShouldReturnExpectedContractShape()
    {
        var response = await _client.GetAsync("/api/roi-validation/rules");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        root.TryGetProperty("version", out var version).Should().BeTrue();
        version.ValueKind.Should().Be(JsonValueKind.String);
        version.GetString().Should().NotBeNullOrWhiteSpace();

        root.TryGetProperty("roiTypes", out var roiTypes).Should().BeTrue();
        roiTypes.ValueKind.Should().Be(JsonValueKind.Array);
        roiTypes.GetArrayLength().Should().BeGreaterThan(0);
    }
}
