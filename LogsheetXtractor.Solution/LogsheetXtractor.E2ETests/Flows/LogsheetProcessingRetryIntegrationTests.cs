using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.E2ETests.Infrastructure;
using LogsheetXtractor.E2ETests.Infrastructure.Fakes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LogsheetXtractor.E2ETests.Flows;

public class LogsheetProcessingRetryIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeHtrScriptEngine _fakeScriptEngine;

    public LogsheetProcessingRetryIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
            }
        );

        var scriptEngine = factory.Services.GetRequiredService<IHtrScriptEngine>();
        _fakeScriptEngine = scriptEngine.Should().BeOfType<FakeHtrScriptEngine>().Subject;
    }

    [Fact]
    public async Task ProcessLogsheet_ShouldRetryAfterTransientFailure_AndSucceedOnSecondAttempt()
    {
        _fakeScriptEngine.ResetProcessingBehavior();

        var templateFileId = await UploadTestFileAsync("retry-template.pdf");
        var logsheetFileId = await UploadTestFileAsync("retry-logsheet.pdf");
        var templateId = await CreateTemplateAsync(templateFileId, "Retry Template");

        await SetRoiAsync(templateId);

        var createdLogsheet = await CreateLogsheetAsync(templateId, logsheetFileId);
        _fakeScriptEngine.ConfigureProcessThrowOnce(
            createdLogsheet.Id,
            new TimeoutException("Simulated transient timeout")
        );

        var startProcessingResponse = await _client.PostAsJsonAsync(
            $"/api/logsheets/{createdLogsheet.Id}/process",
            new { options = new { uglyCheckboxes = false } }
        );
        startProcessingResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var processedLogsheet = await WaitForStatusAsync(createdLogsheet.Id, "NeedsReview");
        processedLogsheet.Status.Should().Be("NeedsReview");
        _fakeScriptEngine.GetProcessAttempts(createdLogsheet.Id).Should().Be(2);
    }

    private async Task<LogsheetDetailResponse> WaitForStatusAsync(Guid logsheetId, string expectedStatus)
    {
        for (var i = 0; i < 25; i++)
        {
            var detail = await GetLogsheetAsync(logsheetId);
            if (detail.Status == expectedStatus)
            {
                return detail;
            }

            await Task.Delay(200);
        }

        var lastState = await GetLogsheetAsync(logsheetId);
        throw new InvalidOperationException(
            $"Logsheet {logsheetId} did not reach status '{expectedStatus}'. Last status: '{lastState.Status}'."
        );
    }

    private async Task<LogsheetDetailResponse> GetLogsheetAsync(Guid logsheetId)
    {
        var response = await _client.GetAsync($"/api/logsheets/{logsheetId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<LogsheetDetailResponse>();
        detail.Should().NotBeNull();
        return detail!;
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

    private async Task SetRoiAsync(Guid templateId)
    {
        var response = await _client.PostAsJsonAsync(
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
                    },
                },
            }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
    private sealed record LogsheetDetailResponse(Guid Id, string Status);
}
