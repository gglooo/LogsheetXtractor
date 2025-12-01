using System.Text.Json;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Services.Credentials;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public class PythonHtrAdapter(
    IScriptExecutor scriptExecutor,
    ICredentialService credentialService,
    IFileStorageService fileStorageService,
    IMapper mapper,
    IConfiguration config) : IHtrScriptEngine
{
    private readonly string _selectRoisOutputPath = "selected_rois.json";

    public async Task<SelectRoisOutputDto> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct)
    {
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

        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.SelectRois,
            $"--pdf_file {inputFilePath} --output_file {outputFilePath} --autodetect --detect_residuals --credentials {usedCredentials} --headless",
            ct);

        var rois = ParseRoisFromFile(outputFilePath, input.TemplateId);

        fileStorageService.DeleteFile(uniqueStoragePath);

        return rois;
    }

    public Task<string> AnnotateRoisAsync(Guid executionId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<LogsheetDetailDto> AutomaticAlignAsync(AutomaticAlignmentInputDto input, CancellationToken ct)
    {
        var logsheetPath = fileStorageService.GetResolvedPath(input.Logsheet.File.StoragePath);
        var templatePath = fileStorageService.GetResolvedPath(input.Logsheet.Template.File.StoragePath);

        var backsideTemplatePath = input.Logsheet.BacksideTemplate is not null
            ? fileStorageService.GetResolvedPath(input.Logsheet.BacksideTemplate.File.StoragePath)
            : null;

        var stdOut = await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.AutomaticAlignment,
            $"--pdf_logsheet {logsheetPath} --pdf_template {templatePath}" +
            (backsideTemplatePath is not null
                ? $" --backside_template {backsideTemplatePath}"
                : ""),
            ct);

        input.Logsheet.AlignmentData = stdOut;

        return mapper.Map<LogsheetDetailDto>(input.Logsheet);
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