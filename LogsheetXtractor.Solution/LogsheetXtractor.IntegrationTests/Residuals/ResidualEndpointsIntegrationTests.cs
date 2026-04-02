using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.IntegrationTests.Residuals;

public class ResidualEndpointsIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ResidualEndpointsIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task ResidualEndpointsWorkflow_ShouldSetListAndUpsertResiduals()
    {
        var templateFileId = await UploadTestFileAsync("residual-template.pdf");
        var templateId = await CreateTemplateAsync(templateFileId, "Residual Workflow Template");

        var setResponse = await _client.PostAsJsonAsync(
            $"/api/templates/{templateId}/residuals/set",
            new
            {
                residuals = new object[]
                {
                    new
                    {
                        id = (Guid?)null,
                        content = "residual-1",
                        coordinates = new { x = 10, y = 20, width = 30, height = 40 },
                    },
                },
            }
        );
        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var setResiduals = await setResponse.Content.ReadFromJsonAsync<List<ResidualResponse>>();
        setResiduals.Should().ContainSingle();
        var createdResidualId = setResiduals![0].Id;
        createdResidualId.Should().NotBeNull();

        var listResponse = await _client.GetAsync($"/api/templates/{templateId}/residuals");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listedResiduals = await listResponse.Content.ReadFromJsonAsync<List<ResidualResponse>>();
        listedResiduals.Should().ContainSingle(r => r.Content == "residual-1");

        var upsertResponse = await _client.PostAsJsonAsync(
            $"/api/templates/{templateId}/residuals/upsert",
            new
            {
                residual = new
                {
                    id = createdResidualId,
                    content = "residual-1-updated",
                    coordinates = new { x = 11, y = 21, width = 31, height = 41 },
                },
            }
        );
        upsertResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var upsertedResidual = await upsertResponse.Content.ReadFromJsonAsync<ResidualResponse>();
        upsertedResidual.Should().NotBeNull();
        upsertedResidual!.Id.Should().Be(createdResidualId);
        upsertedResidual.Content.Should().Be("residual-1-updated");

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/templates/{templateId}/residuals",
            new
            {
                residuals = new object[]
                {
                    new
                    {
                        content = "residual-2",
                        coordinates = new { x = 50, y = 60, width = 70, height = 80 },
                    },
                },
            }
        );
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdResiduals = await createResponse.Content.ReadFromJsonAsync<List<ResidualResponse>>();
        createdResiduals.Should().ContainSingle(r => r.Content == "residual-2");

        var finalListResponse = await _client.GetAsync($"/api/templates/{templateId}/residuals");
        finalListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalResiduals = await finalListResponse.Content.ReadFromJsonAsync<List<ResidualResponse>>();
        finalResiduals.Should().HaveCount(2);
        finalResiduals.Should().Contain(r => r.Content == "residual-1-updated");
        finalResiduals.Should().Contain(r => r.Content == "residual-2");
    }

    [Fact]
    public async Task ListResiduals_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/templates/{Guid.NewGuid()}/residuals");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> UploadTestFileAsync(string fileName)
    {
        using var formContent = new MultipartFormDataContent();
        var content = new ByteArrayContent("integration-test-file"u8.ToArray());
        content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        formContent.Add(content, "formFile", fileName);

        var uploadResponse = await _client.PostAsync("/api/files/upload", formContent);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploadedFile = await uploadResponse.Content.ReadFromJsonAsync<FileUploadResponse>();
        uploadedFile.Should().NotBeNull();
        return uploadedFile!.Id;
    }

    private async Task<Guid> CreateTemplateAsync(Guid fileId, string name)
    {
        var response = await _client.PostAsJsonAsync("/api/templates", new { name, fileId });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var template = await response.Content.ReadFromJsonAsync<TemplateDetailResponse>();
        template.Should().NotBeNull();
        return template!.Id;
    }

    private sealed record FileUploadResponse(Guid Id);
    private sealed record TemplateDetailResponse(Guid Id);
    private sealed record ResidualResponse(Guid? Id, string Content);
}
