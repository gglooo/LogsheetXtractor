using System.Text.Json;
using Microsoft.Extensions.Configuration;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Services.Credentials;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public class PythonHtrAdapter(
    IScriptExecutor scriptExecutor,
    ICredentialService credentialService,
    IFileStorageService fileStorageService,
    IConfiguration config) : IHtrScriptEngine
{
    private readonly string _selectRoisOutputPath = "selected_rois.json";

    public async Task<SelectRoisOutputDto> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct)
    {
        // TODO: get any credentials, not just google
        var availableCredentials = credentialService.GetAvailableCredentialsPath().ToList();

        if (!availableCredentials.Any())
        {
            throw new InvalidOperationException("Credentials not found");
        }

        // Prefer Google credentials if available
        var googleCredentials = availableCredentials
            .FirstOrDefault(c => c.Item1 == ECredentialType.Google);

        var usedCredentials = !googleCredentials.Equals(default)
            ? googleCredentials.Item2
            : availableCredentials.First().Item2;

        var uniqueStoragePath = $"{Guid.NewGuid()}_{_selectRoisOutputPath}";

        var inputFilePath = fileStorageService.GetResolvedPath(input.FilePath);
        var outputFilePath = fileStorageService.GetResolvedPath(uniqueStoragePath);

        await scriptExecutor.ExecuteScriptAsync("select_rois.py",
            $"--pdf_file {inputFilePath} --output_file {outputFilePath} --autodetect --detect_residuals --credentials {usedCredentials}",
            ct);

        var rois = ParseRoisFromFile(outputFilePath, input.TemplateId);

        fileStorageService.DeleteFile(uniqueStoragePath);

        return rois;
    }

    public Task<string> AnnotateRoisAsync(Guid executionId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task ManualAlignAsync(Guid templateId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<ProcessLogsheetOutputDto> ProcessLogsheetAsync(ProcessLogsheetInputDto input, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    private SelectRoisOutputDto ParseRoisFromFile(string filePath, Guid templateId)
    {
        var jsonContent = fileStorageService.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rois = JsonSerializer.Deserialize<PythonSelectRoisOutputDto>(jsonContent, options);

        return rois?.ToSelectRoisOutputDtoList(templateId) ?? new SelectRoisOutputDto([], []);
    }
}