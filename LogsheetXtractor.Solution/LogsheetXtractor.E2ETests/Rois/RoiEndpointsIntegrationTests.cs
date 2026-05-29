using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.E2ETests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.E2ETests.Rois;

public class RoiEndpointsIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RoiEndpointsIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task SetRoisAndGetRois_ShouldPersistData_WhenRequestIsValid()
    {
        var templateId = await TestDataSeeder.SeedEditableTemplateAsync(_factory);

        var setResponse = await _client.PostAsJsonAsync(
            $"/api/templates/{templateId}/rois/set",
            new
            {
                rois = new object[]
                {
                    new
                    {
                        id = (string?)null,
                        variableName = "temperature",
                        type = "Number",
                        coordinates = new { x = 10, y = 20, width = 30, height = 40 },
                        validationCondition = (object?)null,
                    }
                }
            });

        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/templates/{templateId}/rois");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var rois = await getResponse.Content.ReadFromJsonAsync<List<RoiResponse>>();
        rois.Should().NotBeNull();
        rois!.Should().ContainSingle();
        rois[0].VariableName.Should().Be("temperature");
        rois[0].Type.Should().Be("Number");
    }

    [Fact]
    public async Task SetRois_ShouldReturnBadRequest_WhenValidationConditionIsInvalid()
    {
        var templateId = await TestDataSeeder.SeedEditableTemplateAsync(_factory);

        var setResponse = await _client.PostAsJsonAsync(
            $"/api/templates/{templateId}/rois/set",
            new
            {
                rois = new object[]
                {
                    new
                    {
                        id = (string?)null,
                        variableName = "temperature",
                        type = "Number",
                        coordinates = new { x = 10, y = 20, width = 30, height = 40 },
                        validationCondition = new
                        {
                            type = "group",
                            @operator = "AND",
                            children = Array.Empty<object>(),
                        },
                    }
                }
            });

        setResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorBody = await setResponse.Content.ReadAsStringAsync();
        errorBody.Should().Contain("invalid validationCondition");
    }

    private sealed record RoiResponse(
        Guid Id,
        string VariableName,
        Guid TemplateId,
        string Type
    );
}
