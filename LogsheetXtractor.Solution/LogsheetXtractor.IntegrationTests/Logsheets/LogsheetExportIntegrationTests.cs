using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.IntegrationTests.Infrastructure;
using LogsheetXtractor.IntegrationTests.Infrastructure.Fakes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LogsheetXtractor.IntegrationTests.Logsheets;

public class LogsheetExportIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeHtrScriptEngine _fakeScriptEngine;

    public LogsheetExportIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        var scriptEngine = factory.Services.GetRequiredService<IHtrScriptEngine>();
        _fakeScriptEngine = scriptEngine.Should().BeOfType<FakeHtrScriptEngine>().Subject;
    }

    [Fact]
    public async Task ExportLogsheet_ShouldUseRoiVariableName_InsteadOfRoiId()
    {
        _fakeScriptEngine.ResetCapturedExportData();

        var templateFileId = await UploadTestFileAsync("export-template.pdf");
        var logsheetFileId = await UploadTestFileAsync("export-logsheet.pdf");
        var templateId = await CreateTemplateAsync(templateFileId, "Export Variable Name Template");

        await SetRoiAsync(templateId, "temperature");

        var createdLogsheet = await CreateLogsheetAsync(templateId, logsheetFileId);
       
        await WaitForStatusAsync(createdLogsheet.Id, "Pending");

        var startProcessingResponse = await _client.PostAsJsonAsync(
            $"/api/logsheets/{createdLogsheet.Id}/process",
            new
            {
                options = new
                {
                    uglyCheckboxes = false,
                },
            }
        );
        startProcessingResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var processedLogsheet = await WaitForStatusAsync(createdLogsheet.Id, "NeedsReview");
        processedLogsheet.ExtractedValues.Should().ContainSingle();
        var processedRoiId = processedLogsheet.ExtractedValues[0].RoiId;

        foreach (var extractedValue in processedLogsheet.ExtractedValues)
        {
            var verifyResponse = await _client.PostAsJsonAsync(
                $"/api/extracted-values/{extractedValue.Id}/verify",
                new { correctedValue = extractedValue.Value }
            );
            verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var completeProofreadingResponse = await _client.PostAsync(
            $"/api/logsheets/{createdLogsheet.Id}/proofreading/complete",
            content: null
        );
        completeProofreadingResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await WaitForStatusAsync(createdLogsheet.Id, "Completed");

        var exportResponse = await _client.PostAsync(
            $"/api/logsheets/{createdLogsheet.Id}/export",
            content: null
        );
        exportResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _fakeScriptEngine.LastExportData.Should().ContainSingle();
        var exportedField = _fakeScriptEngine.LastExportData.Single();
        exportedField.VariableName.Should().Be("temperature");
        exportedField.VariableName.Should().NotBe(processedRoiId.ToString());
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

    private async Task SetRoiAsync(Guid templateId, string variableName)
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
                        variableName,
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
    private sealed record ExtractedValueResponse(Guid Id, Guid RoiId, string VariableName, string Value);
    private sealed record LogsheetDetailResponse(
        Guid Id,
        string Status,
        List<ExtractedValueResponse> ExtractedValues
    );
}
