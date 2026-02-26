using FluentResults;
using Mapster;
using MapsterMapper;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace WebFormHTR.Infrastructure.Services;

public class LogsheetService(
    IMapper mapper,
    IHtrScriptEngine scriptEngine,
    ILogger<LogsheetService> logger
) : ILogsheetService
{
    private Result ValidateLogsheet(Logsheet? logsheet)
    {
        if (logsheet is null)
        {
            logger.LogWarning("Attempted to validate null logsheet");
            return new NotFoundError("Logsheet not found");
        }

        if (logsheet.Status != ELogSheetStatus.Processing || logsheet.ProcessedAt is not null)
        {
            logger.LogWarning("Logsheet {LogsheetId} is not in a valid state for processing. Status: {Status}",
                logsheet.Id, logsheet.Status);
            return new InvalidStateError("Logsheet is not in a valid state for processing");
        }

        return Result.Ok();
    }

    private async Task<Result> InvokeLogsheetProcessing(Logsheet logsheet, ProcessLogsheetDataOptions? options,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Invoking script engine for Logsheet {LogsheetId}", logsheet.Id);
            var processingOptions = new ProcessLogsheetInputOptionsDto(options?.UglyCheckboxes);
            var outputResult = await scriptEngine.ProcessLogsheetAsync(
                new ProcessLogsheetInputDto(logsheet, processingOptions), ct);

            if (outputResult.IsFailed)
            {
                var errorMessage = outputResult.Errors.FirstOrDefault()?.Message ?? "Unknown error";
                logger.LogError("Script processing failed for Logsheet {LogsheetId}: {Error}", logsheet.Id,
                    errorMessage);
                AdjustAfterFailedProcessing(logsheet, errorMessage);

                return outputResult.ToResult();
            }

            var output = outputResult.Value;

            var extractedData = output.ExtractedData.BuildAdapter()
                .AddParameters("LogsheetId", logsheet.Id)
                .AdaptToType<IEnumerable<ExtractedValue>>()
                .ToList();

            logger.LogInformation("Script processing successful for Logsheet {LogsheetId}. Extracted {Count} values.",
                logsheet.Id, extractedData.Count);

            AdjustAfterSuccessfulProcessing(logsheet, extractedData);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Script processing failed for Logsheet {LogsheetId}", logsheet.Id);
            AdjustAfterFailedProcessing(logsheet, ex.Message);

            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<LogsheetDetailDto>> ProcessLogsheetAsync(Logsheet logsheet,
        ProcessLogsheetDataOptions? options,
        CancellationToken ct)
    {
        var validationResult = ValidateLogsheet(logsheet);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        var result = await InvokeLogsheetProcessing(logsheet, options, ct);

        if (result.IsFailed)
        {
            return Result.Fail<LogsheetDetailDto>(result.Errors);
        }

        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }

    public async Task<Result<IEnumerable<LogsheetDetailDto>>> ProcessLogsheetsAsync(IEnumerable<Logsheet> logsheets,
        ProcessLogsheetDataOptions? options,
        CancellationToken ct)
    {
        var logsheetList = logsheets.ToList();
        logger.LogInformation("Processing batch of {Count} logsheets", logsheetList.Count);

        var processedLogsheets = new List<LogsheetDetailDto>();
        foreach (var logsheet in logsheetList)
        {
            var processedLogsheetResult = await ProcessLogsheetAsync(logsheet, options, ct);
            if (processedLogsheetResult.IsSuccess)
            {
                processedLogsheets.Add(processedLogsheetResult.Value);
            }
            else
            {
                logger.LogWarning("Validation error for Logsheet ID {LogsheetId}: {Message}", logsheet.Id,
                    processedLogsheetResult.Errors.FirstOrDefault()?.Message);
            }
        }

        return Result.Ok(mapper.Map<IEnumerable<LogsheetDetailDto>>(processedLogsheets));
    }

    private void AdjustAfterSuccessfulProcessing(Logsheet logsheet, List<ExtractedValue> extractedData)
    {
        logsheet!.ExtractedValues.Clear();
        extractedData.ForEach(logsheet.ExtractedValues.Add);
        logsheet.ProcessedAt = DateTime.UtcNow;
        logsheet.Status = ELogSheetStatus.NeedsReview;
    }

    private void AdjustAfterFailedProcessing(Logsheet logsheet, string errorMessage)
    {
        logsheet.Status = ELogSheetStatus.Failed;
        logsheet.ErrorMessage = errorMessage;
    }
}