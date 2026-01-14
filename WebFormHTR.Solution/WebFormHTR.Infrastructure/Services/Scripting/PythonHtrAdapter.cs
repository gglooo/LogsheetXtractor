using System.Text;
using System.Text.Json;
using MapsterMapper;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Services.Credentials;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;
using File = WebFormHTR.Domain.Entities.File;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public class PythonHtrAdapter(
    IScriptExecutor scriptExecutor,
    ICredentialService credentialService,
    IFileStorageService fileStorageService,
    IMapper mapper) : IHtrScriptEngine
{
    private readonly string _selectRoisOutputPath = "selected_rois.json";

    public async Task<SelectRoisOutputDto> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct)
    {
        var credentials = credentialService.GetAvailableCredentialsPath().ToList();
        if (credentials.Count == 0)
        {
            throw new InvalidOperationException("No credentials available for ROI selection.");
        }

        var uniqueStoragePath = $"{Guid.NewGuid()}_{_selectRoisOutputPath}";

        var inputFilePath = fileStorageService.GetResolvedPath(input.Template.File.StoragePath);
        var outputFilePath = fileStorageService.GetTemporaryFilePath(uniqueStoragePath);

        var usedCredentials = credentials[0].Item2;
        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.SelectRois,
            $"--pdf_file {inputFilePath} --output_file {outputFilePath} --autodetect --detect_residuals --credentials {usedCredentials} --headless",
            ct);

        var rois = ParseRoisFromFile(outputFilePath, input.Template.Id);

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

    public async Task<ProcessLogsheetOutputDto> ProcessLogsheetAsync(ProcessLogsheetInputDto input,
        CancellationToken ct)
    {
        var credentials = credentialService.GetAvailableCredentialsPath().ToList();
        if (credentials.Count == 0)
        {
            throw new InvalidOperationException("No credentials available for logsheet processing.");
        }

        var logsheet = input.Logsheet;
        var logsheetPath = fileStorageService.GetResolvedPath(logsheet.File.StoragePath);
        var templatePath = fileStorageService.GetResolvedPath(logsheet.Template.File.StoragePath);

        var outputFilePath = fileStorageService.GetTemporaryFilePath($"{Guid.NewGuid()}_processed_logsheet.csv");

        var templateConfig = mapper.Map<PythonTemplateConfig>(logsheet.Template);
        var configJson = JsonSerializer.Serialize(templateConfig);
        var configBytes = Encoding.UTF8.GetBytes(configJson);
        var configPath = await fileStorageService.SaveTemporaryFileAsync(configBytes, Guid.NewGuid() + ".json", ct);

        var credentialsString =
            string.Join(" ", credentials.Select(c => $"--{c.Item1.ToString().ToLower()} {c.Item2}"));

        // TODO: support backside template
        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.ProcessLogsheet,
            $"--output_file {outputFilePath} --pdf_template {templatePath} --pdf_logsheet {logsheetPath} --config_file {configPath} {credentialsString} --aligned --store_csv",
            ct);

        var parsedData = ParseProcessedLogsheetFromCsv(outputFilePath);

        return new ProcessLogsheetOutputDto(parsedData);
    }

    public async Task<PdfDimensionsDto> GetPdfDimensionsAsync(File file, CancellationToken ct)
    {
        var filePath = fileStorageService.GetResolvedPath(file.StoragePath);

        var dimensions = await scriptExecutor.ExecuteScriptWithJsonOutputAsync<PdfDimensionsDto>(
            PythonScriptTypes.PdfDimensions,
            $"--pdf_file {filePath}", ct);

        return dimensions;
    }

    private SelectRoisOutputDto ParseRoisFromFile(string filePath, Guid templateId)
    {
        var jsonContent = fileStorageService.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rois = JsonSerializer.Deserialize<PythonSelectRoisOutputDto>(jsonContent, options);

        return rois?.ToSelectRoisOutputDtoList(templateId) ?? new SelectRoisOutputDto([], []);
    }

    private Dictionary<string, string> ParseProcessedLogsheetFromCsv(string filePath)
    {
        var csvContent = fileStorageService.ReadAllText(filePath);
        var result = new Dictionary<string, string>();

        var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Skip(1);
        foreach (var line in lines)
        {
            var parts = line.Split(',', 2);
            if (parts.Length == 2)
            {
                result[parts[0]] = parts[1];
            }
        }

        return result;
    }
}