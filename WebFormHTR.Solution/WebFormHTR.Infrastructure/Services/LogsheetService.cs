using System.ComponentModel.DataAnnotations;
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
    public Error? ValidateLogsheet(Logsheet? logsheet)
    {
        if (logsheet is null)
        {
            logger.LogWarning("Attempted to validate null logsheet");
            return new NotFoundError("Logsheet not found");
        }

        if (logsheet.Status == ELogSheetStatus.Completed || logsheet.ProcessedAt is not null)
        {
            logger.LogWarning("Logsheet {LogsheetId} is not in a valid state for processing. Status: {Status}", logsheet.Id, logsheet.Status);
            return new InvalidStateError("Logsheet is not in a valid state for processing");
        }

        return null;
    }

    private async Task InvokeLogsheetProcessing(Logsheet logsheet, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Invoking script engine for Logsheet {LogsheetId}", logsheet.Id);
            var output = await scriptEngine.ProcessLogsheetAsync(new ProcessLogsheetInputDto(logsheet), ct);

            var extractedData = output.ExtractedData.BuildAdapter()
                .AddParameters("LogsheetId", logsheet.Id)
                .AdaptToType<IEnumerable<ExtractedValue>>()
                .ToList();
            
            logger.LogInformation("Script processing successful for Logsheet {LogsheetId}. Extracted {Count} values.", logsheet.Id, extractedData.Count);

            AdjustAfterSuccessfulProcessing(logsheet, extractedData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Script processing failed for Logsheet {LogsheetId}", logsheet.Id);
            AdjustAfterFailedProcessing(logsheet, ex.Message);
        }
    }

    public async Task<LogsheetDetailDto> ProcessLogsheetAsync(Logsheet logsheet,
        CancellationToken ct)
    {
        var validationError = ValidateLogsheet(logsheet);
        if (validationError is not null)
        {
            throw new ValidationException(validationError.Message);
        }

        await InvokeLogsheetProcessing(logsheet, ct);

        return mapper.Map<LogsheetDetailDto>(logsheet);
    }

    public async Task<IEnumerable<LogsheetDetailDto>> ProcessLogsheetsAsync(IEnumerable<Logsheet> logsheets,
        CancellationToken ct)
    {
        var logsheetList = logsheets.ToList();
        logger.LogInformation("Processing batch of {Count} logsheets", logsheetList.Count);
        
        var processedLogsheets = new List<LogsheetDetailDto>();
        foreach (var logsheet in logsheetList)
        {
            try
            {
                var processedLogsheet = await ProcessLogsheetAsync(logsheet, ct);
                processedLogsheets.Add(processedLogsheet);
            }
            catch (ValidationException ex)
            {
                logger.LogWarning(ex, "Validation error for Logsheet ID {LogsheetId}: {Message}", logsheet.Id, ex.Message);
            }
        }

        return mapper.Map<IEnumerable<LogsheetDetailDto>>(processedLogsheets);
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