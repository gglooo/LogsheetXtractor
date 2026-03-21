using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.IntegrationTests.Logsheets;

public class LogsheetLifecycleIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LogsheetLifecycleIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task CreateListPatchDeleteLogsheet_ShouldSucceed()
    {
        var templateFileId = await UploadTestFileAsync("lifecycle-template.pdf");
        var logsheetFileId = await UploadTestFileAsync("lifecycle-logsheet.pdf");

        var templateId = await CreateTemplateAsync(templateFileId, "Lifecycle Template");

        var createLogsheetResponse = await _client.PostAsJsonAsync(
            "/api/logsheets",
            new
            {
                templateId,
                backsideTemplateId = (Guid?)null,
                fileId = logsheetFileId,
                performAutomaticAlignment = false,
            });

        createLogsheetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdLogsheet = await createLogsheetResponse.Content.ReadFromJsonAsync<LogsheetDetailResponse>();
        createdLogsheet.Should().NotBeNull();
        createdLogsheet!.Id.Should().NotBe(Guid.Empty);
        createdLogsheet.Template.Id.Should().Be(templateId);
        createdLogsheet.File.Id.Should().Be(logsheetFileId);

        var listResponse = await _client.GetAsync($"/api/templates/{templateId}/logsheets");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listedLogsheets = await listResponse.Content.ReadFromJsonAsync<List<LogsheetListResponse>>();
        listedLogsheets.Should().Contain(ls => ls.Id == createdLogsheet.Id);

        var getResponse = await _client.GetAsync($"/api/logsheets/{createdLogsheet.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var patchResponse = await _client.PatchAsJsonAsync(
            $"/api/logsheets/{createdLogsheet.Id}",
            new
            {
                frontAlignmentData = "{\"x\":1}",
                backAlignmentData = (string?)null,
            });
        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/logsheets/{createdLogsheet.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/logsheets/{createdLogsheet.Id}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var getDeletedFileResponse = await _client.GetAsync($"/api/files/{logsheetFileId}");
        getDeletedFileResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateLogsheet_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        var logsheetFileId = await UploadTestFileAsync("missing-template-logsheet.pdf");

        var response = await _client.PostAsJsonAsync(
            "/api/logsheets",
            new
            {
                templateId = Guid.NewGuid(),
                backsideTemplateId = (Guid?)null,
                fileId = logsheetFileId,
                performAutomaticAlignment = false,
            });

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
        var response = await _client.PostAsJsonAsync(
            "/api/templates",
            new
            {
                name,
                fileId,
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var template = await response.Content.ReadFromJsonAsync<TemplateDetailResponse>();
        template.Should().NotBeNull();
        return template!.Id;
    }

    private sealed record FileUploadResponse(Guid Id);
    private sealed record TemplateRef(Guid Id);
    private sealed record FileRef(Guid Id);
    private sealed record TemplateDetailResponse(Guid Id);
    private sealed record LogsheetDetailResponse(Guid Id, TemplateRef Template, FileRef File);
    private sealed record LogsheetListResponse(Guid Id);
}
