using System.Text;
using System.Text.Json;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Services.Credentials;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;
using File = WebFormHTR.Domain.Entities.File;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public class PythonHtrAdapter(
    IScriptExecutor scriptExecutor,
    ICredentialService credentialService,
    IFileStorageService fileStorageService,
    IMapper mapper,
    IScriptInputPreparer inputPreparer,
    IScriptOutputParser outputParser,
    ILogger<PythonHtrAdapter> logger) : IHtrScriptEngine
{
    private readonly string _selectRoisOutputPath = "selected_rois.json";

    public async Task<SelectRoisOutputDto> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct)
    {
        logger.LogInformation("Starting ROI selection for Template: {TemplateId}", input.Template.Id);
        var credentials = credentialService.GetAvailableCredentialsPath().ToList();
        if (credentials.Count == 0)
        {
            logger.LogError("No credentials available for ROI selection.");
            throw new InvalidOperationException("No credentials available for ROI selection.");
        }

        var uniqueStoragePath = $"{Guid.NewGuid()}_{_selectRoisOutputPath}";

        var inputFilePath = fileStorageService.GetResolvedPath(input.Template.File.StoragePath);
        var outputFilePath = fileStorageService.GetTemporaryFilePath(uniqueStoragePath);

        var usedCredentials = credentials[0].Item2;
        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.SelectRois,
            $"--pdf_file {inputFilePath} --output_file {outputFilePath} --autodetect --detect_residuals --credentials {usedCredentials} --headless",
            ct);

        var rois = outputParser.ParseSelectRoisJson(outputFilePath, input.Template.Id);
        logger.LogInformation("ROI selection completed. Found {Count} ROIs.", rois.Rois.Count());

        return rois;
    }

    public Task<string> AnnotateRoisAsync(Guid executionId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<LogsheetDetailDto> AutomaticAlignAsync(AutomaticAlignmentInputDto input, CancellationToken ct)
    {
        logger.LogInformation("Starting automatic alignment for Logsheet: {LogsheetId}", input.Logsheet.Id);
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

        logger.LogInformation("Automatic alignment completed for Logsheet: {LogsheetId}", input.Logsheet.Id);

        return mapper.Map<LogsheetDetailDto>(input.Logsheet);
    }

    public async Task<ProcessLogsheetOutputDto> ProcessLogsheetAsync(ProcessLogsheetInputDto input,
        CancellationToken ct)
    {
        logger.LogInformation("Processing logsheet: {LogsheetId}", input.Logsheet.Id);
        var credentials = credentialService.GetAvailableCredentialsPath().ToList();
        if (credentials.Count == 0)
        {
            logger.LogError("No credentials available for ProcessLogsheet.");
            throw new InvalidOperationException("No credentials available for logsheet processing.");
        }

        var logsheet = input.Logsheet;
        var logsheetPath = fileStorageService.GetResolvedPath(logsheet.File.StoragePath);
        var templatePath = fileStorageService.GetResolvedPath(logsheet.Template.File.StoragePath);

        var outputFilePath = fileStorageService.GetTemporaryFilePath($"{Guid.NewGuid()}_processed_logsheet.csv");

        var configPath = await inputPreparer.CreateTemplateConfigAsync(logsheet.Template, ct);

        var credentialsString =
            string.Join(" ", credentials.Select(c => $"--{c.Item1.ToString().ToLower()} {c.Item2}"));

        // TODO: support backside template
        var alignmentArgument = await inputPreparer.CreateAlignmentArgumentAsync(logsheet, ct);

        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.ProcessLogsheet,
            $"--output_file {outputFilePath} --pdf_template {templatePath} --pdf_logsheet {logsheetPath} --config_file {configPath} {credentialsString} {alignmentArgument} --store_csv",
            ct);

        var parsedData = outputParser.ParseProcessLogsheetCsv(outputFilePath);

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

    public async Task<GetFileDto> ExportLogsheetDataAsync(Logsheet logsheet,
        IEnumerable<ExportLogsheetDataDto> data,
        File logsheetFile,
        File templateFile,
        CancellationToken ct)
    {
        var filePath = fileStorageService.GetResolvedPath(logsheetFile.StoragePath);
        var templatePath = fileStorageService.GetResolvedPath(templateFile.StoragePath);

        var alignmentArgument = await inputPreparer.CreateAlignmentArgumentAsync(logsheet, ct);

        var payload = new ExportLogsheetPayloadDto
        {
            Data = data.ToList(),
            Width = logsheet.Template.Width ?? 0,
            Height = logsheet.Template.Height ?? 0
        };

        var configJson = JsonSerializer.Serialize(payload);
        var configBytes = Encoding.UTF8.GetBytes(configJson);

        var configName = $"{Guid.NewGuid()}_export_config.json";
        var configPath = await fileStorageService.SaveTemporaryFileAsync(configBytes, configName, ct);

        var outputFilePath = fileStorageService.GetTemporaryFilePath($"{Guid.NewGuid()}_exported_logsheet.csv");
        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.ExportLogsheet,
            $"--pdf_logsheet {filePath} --pdf_template {templatePath} --config_file {configPath} --output_file {outputFilePath} {alignmentArgument}",
            ct);

        var fileStream = fileStorageService.GetTemporaryFile(outputFilePath);
        return new GetFileDto
        {
            FileName = $"exported_logsheet_{Guid.NewGuid()}.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Stream = fileStream
        };
    }
}