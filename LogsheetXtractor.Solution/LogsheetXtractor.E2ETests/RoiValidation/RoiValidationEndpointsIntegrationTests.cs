using System.Net;
using FluentAssertions;
using LogsheetXtractor.E2ETests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.E2ETests.RoiValidation;

public class RoiValidationEndpointsIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoiValidationEndpointsIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task GetPredefinedConditions_ShouldReturnBadRequest_WhenRoiTypeIsInvalid()
    {
        var response = await _client.GetAsync("/api/roi-validation/predefined-conditions?roiType=InvalidType");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPredefinedConditions_ShouldReturnOk_WhenRoiTypeIsValid()
    {
        var response = await _client.GetAsync("/api/roi-validation/predefined-conditions?roiType=Number");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPredefinedConditions_ShouldReturnOk_WhenRoiTypeIsMissing()
    {
        var response = await _client.GetAsync("/api/roi-validation/predefined-conditions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
