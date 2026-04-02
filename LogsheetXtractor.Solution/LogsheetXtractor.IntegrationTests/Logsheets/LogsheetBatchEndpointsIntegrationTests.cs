using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogsheetXtractor.IntegrationTests.Logsheets;

public class LogsheetBatchEndpointsIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LogsheetBatchEndpointsIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task DeleteBatchLogsheets_ShouldDeleteAllRequestedLogsheetsAndFiles()
    {
        var templateFileId = await UploadTestFileAsync("batch-template.pdf");
        var logsheetFile1 = await UploadTestFileAsync("batch-logsheet-1.pdf");
        var logsheetFile2 = await UploadTestFileAsync("batch-logsheet-2.pdf");
        var templateId = await CreateTemplateAsync(templateFileId, "Batch Delete Template");

        var logsheet1 = await CreateLogsheetAsync(templateId, logsheetFile1);
        var logsheet2 = await CreateLogsheetAsync(templateId, logsheetFile2);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/logsheets/batch")
        {
            Content = JsonContent.Create(
                new
                {
                    logsheetIds = new[] { logsheet1.Id, logsheet2.Id },
                }
            ),
        };
        var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getLogsheet1Response = await _client.GetAsync($"/api/logsheets/{logsheet1.Id}");
        var getLogsheet2Response = await _client.GetAsync($"/api/logsheets/{logsheet2.Id}");
        getLogsheet1Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        getLogsheet2Response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var getFile1Response = await _client.GetAsync($"/api/files/{logsheetFile1}");
        var getFile2Response = await _client.GetAsync($"/api/files/{logsheetFile2}");
        getFile1Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        getFile2Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBatchLogsheets_ShouldReturnNotFound_WhenAnyLogsheetIsMissing()
    {
        var templateFileId = await UploadTestFileAsync("batch-template-notfound.pdf");
        var logsheetFileId = await UploadTestFileAsync("batch-logsheet-existing.pdf");
        var templateId = await CreateTemplateAsync(templateFileId, "Batch Delete NotFound Template");
        var existingLogsheet = await CreateLogsheetAsync(templateId, logsheetFileId);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/logsheets/batch")
        {
            Content = JsonContent.Create(
                new
                {
                    logsheetIds = new[] { existingLogsheet.Id, Guid.NewGuid() },
                }
            ),
        };
        var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var existingLogsheetResponse = await _client.GetAsync($"/api/logsheets/{existingLogsheet.Id}");
        existingLogsheetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
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

    private async Task<LogsheetDetailResponse> CreateLogsheetAsync(Guid templateId, Guid fileId)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/logsheets",
            new
            {
                templateId,
                backsideTemplateId = (Guid?)null,
                fileId,
                performAutomaticAlignment = false,
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var logsheet = await response.Content.ReadFromJsonAsync<LogsheetDetailResponse>();
        logsheet.Should().NotBeNull();
        return logsheet!;
    }

    private sealed record FileUploadResponse(Guid Id);
    private sealed record TemplateDetailResponse(Guid Id);
    private sealed record LogsheetDetailResponse(Guid Id);
}
