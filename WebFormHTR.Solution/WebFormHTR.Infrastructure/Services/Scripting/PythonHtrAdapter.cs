using System.Text.Json;
using Microsoft.Extensions.Configuration;
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
        var googleCredentials = credentialService.GetCredentialFilePath(ECredentialType.Google);

        if (googleCredentials is null)
        {
            throw new InvalidOperationException("Credentials not found");
        }

        var uniqueStoragePath = $"{Guid.NewGuid()}_{_selectRoisOutputPath}";

        var inputFilePath = fileStorageService.GetResolvedPath(input.FilePath);
        var outputFilePath = fileStorageService.GetResolvedPath(uniqueStoragePath);
        var result =
            await scriptExecutor.ExecuteScriptAsync("select_rois.py",
                $"--pdf_file {inputFilePath} --output_file {outputFilePath} --autodetect --credentials {googleCredentials.Value.Item2}",
                ct);

        var rois = LoadRoisFromFile(outputFilePath, input.TemplateId);

        fileStorageService.DeleteFile(uniqueStoragePath);

        return new SelectRoisOutputDto(rois);
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

    private List<RoiDto> LoadRoisFromFile(string filePath, Guid templateId)
    {
        var jsonContent = fileStorageService.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rois = JsonSerializer.Deserialize<PythonSelectRoisOutputDto>(jsonContent, options);

        return rois?.ToRoiDtoList(templateId) ?? [];
    }
}