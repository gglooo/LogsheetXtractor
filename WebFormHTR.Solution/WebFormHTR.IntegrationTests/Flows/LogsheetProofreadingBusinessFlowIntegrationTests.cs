using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WebFormHTR.IntegrationTests.Infrastructure;

namespace WebFormHTR.IntegrationTests.Flows;

public class LogsheetProofreadingBusinessFlowIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LogsheetProofreadingBusinessFlowIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task LogsheetProcessingAndProofreadingFlow_ShouldFollowBusinessStateTransitions()
    {
        var templateFileId = await UploadTestFileAsync("business-flow-template.pdf");
        var logsheetFileId = await UploadTestFileAsync("business-flow-logsheet.pdf");
        var templateId = await CreateTemplateAsync(templateFileId, "Business Flow Template");

        await SetRoiAsync(templateId);

        var createdLogsheet = await CreateLogsheetAsync(templateId, logsheetFileId);

        var startProcessingResponse = await _client.PostAsJsonAsync(
            $"/api/logsheets/{createdLogsheet.Id}/process",
            new
            {
                options = new
                {
                    uglyCheckboxes = false,
                },
            });
        startProcessingResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var processedLogsheet = await WaitForStatusAsync(createdLogsheet.Id, "NeedsReview");
        processedLogsheet.ExtractedValues.Should().NotBeNullOrEmpty();

        var completeBeforeVerificationResponse = await _client.PostAsync(
            $"/api/logsheets/{createdLogsheet.Id}/proofreading/complete",
            content: null);
        completeBeforeVerificationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        foreach (var extractedValue in processedLogsheet.ExtractedValues)
        {
            var verifyResponse = await _client.PostAsJsonAsync(
                $"/api/extracted-values/{extractedValue.Id}/verify",
                new { correctedValue = extractedValue.Value });
            verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var completeAfterVerificationResponse = await _client.PostAsync(
            $"/api/logsheets/{createdLogsheet.Id}/proofreading/complete",
            content: null);
        completeAfterVerificationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var completedLogsheet = await GetLogsheetAsync(createdLogsheet.Id);
        completedLogsheet.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Processing_ShouldAttachValidationWarnings_WhenExtractedValuesDoNotMeetRoiRules()
    {
        var templateFileId = await UploadTestFileAsync("rule-flow-template.pdf");
        var logsheetFileId = await UploadTestFileAsync("rule-flow-logsheet.pdf");
        var templateId = await CreateTemplateAsync(templateFileId, "Rule Flow Template");

        var setRoisResponse = await _client.PostAsJsonAsync(
            $"/api/templates/{templateId}/rois/set",
            new
            {
                rois = new object[]
                {
                    new
                    {
                        id = (string?)null,
                        variableName = "max_ten",
                        type = "Number",
                        coordinates = new { x = 10, y = 20, width = 30, height = 40 },
                        validationCondition = new
                        {
                            type = "group",
                            @operator = "AND",
                            children = new object[]
                            {
                                new
                                {
                                    type = "rule",
                                    ruleType = "number.range",
                                    @params = new
                                    {
                                        min = (decimal?)null,
                                        max = 10,
                                        inclusiveMin = true,
                                        inclusiveMax = true,
                                    },
                                },
                            },
                        },
                    },
                    new
                    {
                        id = (string?)null,
                        variableName = "min_hundred",
                        type = "Number",
                        coordinates = new { x = 60, y = 20, width = 30, height = 40 },
                        validationCondition = new
                        {
                            type = "group",
                            @operator = "AND",
                            children = new object[]
                            {
                                new
                                {
                                    type = "rule",
                                    ruleType = "number.range",
                                    @params = new
                                    {
                                        min = 100,
                                        max = (decimal?)null,
                                        inclusiveMin = true,
                                        inclusiveMax = true,
                                    },
                                },
                            },
                        },
                    },
                }
            });
        setRoisResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdLogsheet = await CreateLogsheetAsync(templateId, logsheetFileId);

        var startProcessingResponse = await _client.PostAsJsonAsync(
            $"/api/logsheets/{createdLogsheet.Id}/process",
            new { options = new { uglyCheckboxes = false } });
        startProcessingResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var processedLogsheet = await WaitForStatusAsync(createdLogsheet.Id, "NeedsReview");

        var maxTenValue = processedLogsheet.ExtractedValues.Single(v => v.VariableName == "max_ten");
        maxTenValue.ValidationWarnings.Should().ContainSingle();
        maxTenValue.ValidationWarnings[0].Code.Should().Be("number.range");
        maxTenValue.ValidationWarnings[0].Message.Should().Be("Value must be less than or equal to 10.");
        maxTenValue.ValidationWarnings[0].Path.Should().Be("root.children[0]");

        var minHundredValue = processedLogsheet.ExtractedValues.Single(v => v.VariableName == "min_hundred");
        minHundredValue.ValidationWarnings.Should().ContainSingle();
        minHundredValue.ValidationWarnings[0].Code.Should().Be("number.range");
        minHundredValue.ValidationWarnings[0].Message.Should().Be("Value must be greater than or equal to 100.");
        minHundredValue.ValidationWarnings[0].Path.Should().Be("root.children[0]");
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
            $"Logsheet {logsheetId} did not reach status '{expectedStatus}'. Last status: '{lastState.Status}'.");
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
                    }
                }
            });

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
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var logsheet = await response.Content.ReadFromJsonAsync<LogsheetDetailResponse>();
        logsheet.Should().NotBeNull();
        return logsheet!;
    }

    private sealed record FileUploadResponse(Guid Id);
    private sealed record TemplateDetailResponse(Guid Id);
    private sealed record ValidationWarningResponse(string Code, string Message, string Path);
    private sealed record ExtractedValueResponse(
        Guid Id,
        string VariableName,
        string Value,
        string Status,
        List<ValidationWarningResponse> ValidationWarnings
    );
    private sealed record LogsheetDetailResponse(Guid Id, string Status, List<ExtractedValueResponse> ExtractedValues);
}
