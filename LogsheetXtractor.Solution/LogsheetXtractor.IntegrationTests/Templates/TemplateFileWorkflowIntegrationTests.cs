using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.IntegrationTests.Templates;

public class TemplateFileWorkflowIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TemplateFileWorkflowIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task UploadCreateTemplateAndRoiWorkflow_ShouldSucceed()
    {
        var fileId = await UploadTestFileAsync("workflow-template.pdf");

        var createTemplateResponse = await _client.PostAsJsonAsync(
            "/api/templates",
            new
            {
                name = "Workflow Template",
                fileId,
            });

        createTemplateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdTemplate = await createTemplateResponse.Content.ReadFromJsonAsync<TemplateDetailResponse>();
        createdTemplate.Should().NotBeNull();
        createdTemplate!.Id.Should().NotBe(Guid.Empty);
        createdTemplate.Name.Should().Be("Workflow Template");
        createdTemplate.File!.Id.Should().Be(fileId);
        createdTemplate.Width.Should().Be(1000);
        createdTemplate.Height.Should().Be(1400);

        var setRoisResponse = await _client.PostAsJsonAsync(
            $"/api/templates/{createdTemplate.Id}/rois/set",
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

        setRoisResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getTemplateResponse = await _client.GetAsync($"/api/templates/{createdTemplate.Id}");
        getTemplateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var templateDetail = await getTemplateResponse.Content.ReadFromJsonAsync<TemplateDetailResponse>();
        templateDetail.Should().NotBeNull();
        templateDetail!.Rois.Should().ContainSingle(r => r.VariableName == "temperature");

        var listTemplatesResponse = await _client.GetAsync("/api/templates?search=Workflow%20Template");
        listTemplatesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var templates = await listTemplatesResponse.Content.ReadFromJsonAsync<List<TemplateListResponse>>();
        templates.Should().Contain(t => t.Id == createdTemplate.Id);

        var getRoisResponse = await _client.GetAsync($"/api/templates/{createdTemplate.Id}/rois");
        getRoisResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rois = await getRoisResponse.Content.ReadFromJsonAsync<List<RoiResponse>>();
        rois.Should().ContainSingle(r => r.VariableName == "temperature" && r.Type == "Number");
    }

    [Fact]
    public async Task UploadFile_ShouldReturnBadRequest_WhenNoFileContentProvided()
    {
        using var formContent = new MultipartFormDataContent();
        var emptyFile = new ByteArrayContent([]);
        emptyFile.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        formContent.Add(emptyFile, "formFile", "empty.pdf");

        var response = await _client.PostAsync("/api/files/upload", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        uploadedFile!.Id.Should().NotBe(Guid.Empty);

        return uploadedFile.Id;
    }

    private sealed record FileUploadResponse(Guid Id);

    private sealed record TemplateFileResponse(Guid Id);

    private sealed record RoiResponse(Guid Id, string VariableName, string Type);

    private sealed record TemplateDetailResponse(
        Guid Id,
        string Name,
        int Width,
        int Height,
        TemplateFileResponse? File,
        List<RoiResponse> Rois
    );

    private sealed record TemplateListResponse(Guid Id, string Name);
}
