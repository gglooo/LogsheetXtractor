using System.Text;
using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Extensions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.PdfCropper;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;
using LogsheetXtractor.Infrastructure.Services.Storage;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using File = LogsheetXtractor.Domain.Entities.File;

namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public class PythonHtrAdapter(
    IScriptExecutor scriptExecutor,
    ICredentialContextProvider credentialContextProvider,
    IFileStorageService fileStorageService,
    IMapper mapper,
    IPythonScriptArgumentsBuilder scriptArgumentsBuilder,
    IScriptInputPreparer inputPreparer,
    IScriptOutputParser outputParser,
    IPdfCropperService pdfCropperService,
    IFileService fileService,
    ILogger<PythonHtrAdapter> logger
) : IHtrScriptEngine
{
    private readonly string _selectRoisOutputPath = "selected_rois.json";

    public async Task<Result<SelectRoisOutputDto>> SelectRoisAsync(
        SelectRoisInputDto input,
        CancellationToken ct
    )
    {
        logger.LogInformation(
            "Starting ROI selection for Template: {TemplateId}",
            input.Template.Id
        );

        var contextResult = await credentialContextProvider.GetCredentialContextAsync(ct);
        if (contextResult.IsFailed)
        {
            return Result.Fail<SelectRoisOutputDto>(contextResult.Errors);
        }

        await using var context = contextResult.Value;
        var credentials = context.CredentialPaths.ToList();
        var areGoogleCredentialsAvailable = credentials.Any(c => c.Item1 == ECredentialType.Google);
        if (!areGoogleCredentialsAvailable)
        {
            logger.LogError(
                "No credentials available for ROI selection. Skipping residual detection."
            );
        }

        var uniqueStoragePath = $"{Guid.NewGuid()}_{_selectRoisOutputPath}";

        var inputFilePath = fileStorageService.GetResolvedPath(input.Template.File.StoragePath);
        var outputFilePath = fileStorageService.GetTemporaryFilePath(uniqueStoragePath);

        var usedCredentials = credentials
            .FirstOrDefault(c => c.Item1 == ECredentialType.Google)
            .Item2;
        var args = scriptArgumentsBuilder.BuildSelectRoisArguments(
            inputFilePath,
            outputFilePath,
            usedCredentials
        );

        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.SelectRois, args, ct);

        var rois = await outputParser.ParseSelectRoisJsonAsync(
            outputFilePath,
            input.Template.Id,
            ct
        );
        logger.LogInformation("ROI selection completed. Found {Count} ROIs.", rois.Rois.Count());

        return Result.Ok(rois);
    }

    public async Task<Result<LogsheetDetailDto>> AutomaticAlignAsync(
        AutomaticAlignmentInputDto input,
        CancellationToken ct
    )
    {
        logger.LogInformation(
            "Starting automatic alignment for Logsheet: {LogsheetId}",
            input.Logsheet.Id
        );
        var logsheetPath = fileStorageService.GetResolvedPath(input.Logsheet.File.StoragePath);
        var pdfStream = (await fileService.GetFileAsync(input.Logsheet.FileId))?.Stream;
        if (pdfStream is null)
        {
            logger.LogWarning("Logsheet file not found for LogsheetId: {Id}", input.Logsheet.Id);
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        if (input.Logsheet.Template.Width is null || input.Logsheet.Template.Height is null)
        {
            logger.LogError(
                "Template dimensions are required for automatic alignment. TemplateId: {TemplateId}",
                input.Logsheet.Template.Id
            );
            return Result.Fail(
                new InvalidStateError("Template dimensions are required for automatic alignment.")
            );
        }

        var pdfBytes = pdfStream.ToByteArray();
        var hasBacksidePage = pdfCropperService.GetPageCount(pdfBytes, ct) > 1;

        var templatePath = fileStorageService.GetResolvedPath(
            input.Logsheet.Template.File.StoragePath
        );

        var backsideTemplate = input.Logsheet.Template.BacksideTemplate;
        var backsideTemplatePath = backsideTemplate is not null
            ? fileStorageService.GetResolvedPath(backsideTemplate.File.StoragePath)
            : null;

        var argsList = scriptArgumentsBuilder.BuildAutomaticAlignmentArguments(
            logsheetPath,
            templatePath,
            hasBacksidePage ? backsideTemplatePath : null
        );

        var stdOut = await scriptExecutor.ExecuteScriptAsync(
            PythonScriptTypes.AutomaticAlignment,
            argsList,
            ct
        );

        input.Logsheet.AlignmentData = outputParser.ParseAutomaticAlignmentJson(
            stdOut,
            // Using null coalescing just because the types are nullable, we check that these values are not null above
            input.Logsheet.Template.Width ?? 0,
            input.Logsheet.Template.Height ?? 0,
            backsideTemplate?.Width,
            backsideTemplate?.Height
        );

        logger.LogInformation(
            "Automatic alignment completed for Logsheet: {LogsheetId}",
            input.Logsheet.Id
        );

        return Result.Ok(mapper.Map<LogsheetDetailDto>(input.Logsheet));
    }

    public async Task<Result<ProcessLogsheetOutputDto>> ProcessLogsheetAsync(
        ProcessLogsheetInputDto input,
        CancellationToken ct
    )
    {
        logger.LogInformation("Processing logsheet: {LogsheetId}", input.Logsheet.Id);

        var contextResult = await credentialContextProvider.GetCredentialContextAsync(ct);
        if (contextResult.IsFailed)
        {
            return Result.Fail<ProcessLogsheetOutputDto>(contextResult.Errors);
        }

        await using var context = contextResult.Value;
        var credentials = context.CredentialPaths.ToList();

        if (credentials.Count == 0)
        {
            logger.LogError("No credentials available for ProcessLogsheet.");
            return Result.Fail(
                new InvalidStateError("No credentials available for logsheet processing.")
            );
        }

        var logsheet = input.Logsheet;
        var logsheetPath = fileStorageService.GetResolvedPath(logsheet.File.StoragePath);
        var pdfStream = (await fileService.GetFileAsync(logsheet.FileId))?.Stream;
        if (pdfStream is null)
        {
            logger.LogWarning("Logsheet file not found for LogsheetId: {Id}", logsheet.Id);
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        var pdfBytes = pdfStream.ToByteArray();
        var hasBacksidePage = pdfCropperService.GetPageCount(pdfBytes, ct) > 1;

        var templatePath = fileStorageService.GetResolvedPath(logsheet.Template.File.StoragePath);

        var outputFilePath = fileStorageService.GetTemporaryFilePath(
            $"{Guid.NewGuid()}_processed_logsheet.csv"
        );

        var configPath = await inputPreparer.CreateTemplateConfigAsync(logsheet.Template, ct);

        var alignmentInput = await inputPreparer.PrepareAlignmentInputAsync(
            logsheet,
            hasBacksidePage,
            ct
        );
        var backsideInput = await inputPreparer.PrepareBacksideInputAsync(
            logsheet,
            hasBacksidePage,
            ct
        );

        var argsList = scriptArgumentsBuilder.BuildProcessLogsheetArguments(
            outputFilePath,
            templatePath,
            logsheetPath,
            configPath,
            credentials,
            alignmentInput,
            backsideInput,
            input?.Options?.UglyCheckboxes == true
        );

        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.ProcessLogsheet, argsList, ct);

        var parsedData = await outputParser.ParseProcessLogsheetCsvAsync(outputFilePath, ct);

        return Result.Ok(new ProcessLogsheetOutputDto(parsedData));
    }

    public async Task<Result<PdfDimensionsDto>> GetPdfDimensionsAsync(
        File file,
        CancellationToken ct
    )
    {
        var filePath = fileStorageService.GetResolvedPath(file.StoragePath);

        var dimensions = await scriptExecutor.ExecuteScriptWithJsonOutputAsync<PdfDimensionsDto>(
            PythonScriptTypes.PdfDimensions,
            scriptArgumentsBuilder.BuildPdfDimensionsArguments(filePath),
            ct
        );

        return Result.Ok(dimensions);
    }

    public async Task<Result<GetFileDto>> ExportLogsheetDataAsync(
        Logsheet logsheet,
        IEnumerable<ExportLogsheetDataDto> data,
        File logsheetFile,
        File templateFile,
        CancellationToken ct
    )
    {
        logger.LogInformation("Exporting data for Logsheet {LogsheetId}", logsheet.Id);
        var filePath = fileStorageService.GetResolvedPath(logsheetFile.StoragePath);
        var pdfStream = (await fileService.GetFileAsync(logsheet.FileId))?.Stream;
        if (pdfStream is null)
        {
            logger.LogWarning("Logsheet file not found for LogsheetId: {Id}", logsheet.Id);
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        var pdfBytes = pdfStream.ToByteArray();
        var hasBacksidePage = pdfCropperService.GetPageCount(pdfBytes, ct) > 1;

        var templatePath = fileStorageService.GetResolvedPath(templateFile.StoragePath);

        var alignmentInput = await inputPreparer.PrepareAlignmentInputAsync(
            logsheet,
            hasBacksidePage,
            ct
        );

        var payload = new ExportLogsheetPayloadDto
        {
            Data = data.ToList(),
            Width = logsheet.Template.Width ?? 0,
            Height = logsheet.Template.Height ?? 0,
        };

        var configJson = JsonSerializer.Serialize(payload);
        var configBytes = Encoding.UTF8.GetBytes(configJson);

        var configName = $"{Guid.NewGuid()}_export_config.json";
        var configPath = await fileStorageService.SaveTemporaryFileAsync(
            configBytes,
            configName,
            ct
        );

        var outputFilePath = fileStorageService.GetTemporaryFilePath(
            $"{Guid.NewGuid()}_exported_logsheet.csv"
        );

        var backsideInput = await inputPreparer.PrepareBacksideInputAsync(
            logsheet,
            hasBacksidePage,
            ct
        );

        var argsList = scriptArgumentsBuilder.BuildExportLogsheetArguments(
            filePath,
            templatePath,
            configPath,
            outputFilePath,
            alignmentInput,
            backsideInput
        );

        logger.LogInformation("Executing export script for Logsheet {LogsheetId}", logsheet.Id);
        await scriptExecutor.ExecuteScriptAsync(PythonScriptTypes.ExportLogsheet, argsList, ct);

        logger.LogInformation(
            "Export script completed successfully for Logsheet {LogsheetId}",
            logsheet.Id
        );

        var fileStream = fileStorageService.GetTemporaryFile(outputFilePath);

        return Result.Ok(
            new GetFileDto
            {
                FileName = $"exported_logsheet_{Guid.NewGuid()}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Stream = fileStream,
            }
        );
    }
}
