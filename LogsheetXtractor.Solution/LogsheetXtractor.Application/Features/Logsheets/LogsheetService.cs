using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.RoiValidation;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Application.Features.Logsheets;

public class LogsheetService(
    IMapper mapper,
    IHtrScriptEngine scriptEngine,
    IRoiValidationConditionEvaluator roiValidationConditionEvaluator,
    IRoiValidationRuleCatalogProvider roiValidationRuleCatalogProvider,
    ILogger<LogsheetService> logger
) : ILogsheetService
{
    public async Task<Result<LogsheetDetailDto>> AlignLogsheetAsync(
        Logsheet logsheet,
        CancellationToken ct
    )
    {
        try
        {
            logger.LogInformation(
                "Invoking automatic alignment for Logsheet {LogsheetId}",
                logsheet.Id
            );
            var alignmentResult = await scriptEngine.AutomaticAlignAsync(
                new AutomaticAlignmentInputDto(logsheet),
                ct
            );

            if (alignmentResult.IsFailed)
            {
                var errorMessage =
                    alignmentResult.Errors.FirstOrDefault()?.Message ?? "Unknown error";
                logger.LogError(
                    "Automatic alignment failed for Logsheet {LogsheetId}: {Error}",
                    logsheet.Id,
                    errorMessage
                );
            }

            return alignmentResult;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Automatic alignment failed for Logsheet {LogsheetId}",
                logsheet.Id
            );
            return Result.Fail<LogsheetDetailDto>($"Failed to align logsheet: {ex.Message}");
        }
    }

    private Result ValidateLogsheetForProcessing(Logsheet? logsheet)
    {
        if (logsheet is null)
        {
            logger.LogWarning("Attempted to validate null logsheet");
            return new NotFoundError("Logsheet not found");
        }

        if (logsheet.Status != ELogSheetStatus.Processing || logsheet.ProcessedAt is not null)
        {
            logger.LogWarning(
                "Logsheet {LogsheetId} is not in a valid state for processing. Status: {Status}",
                logsheet.Id,
                logsheet.Status
            );
            return new InvalidStateError("Logsheet is not in a valid state for processing");
        }

        return Result.Ok();
    }

    private async Task<Result> InvokeLogsheetProcessing(
        Logsheet logsheet,
        ProcessLogsheetDataOptions? options,
        CancellationToken ct
    )
    {
        try
        {
            logger.LogInformation("Invoking script engine for Logsheet {LogsheetId}", logsheet.Id);
            var processingOptions = new ProcessLogsheetInputOptionsDto(options?.UglyCheckboxes);
            var outputResult = await scriptEngine.ProcessLogsheetAsync(
                new ProcessLogsheetInputDto(logsheet, processingOptions),
                ct
            );

            if (outputResult.IsFailed)
            {
                var errorMessage = outputResult.Errors.FirstOrDefault()?.Message ?? "Unknown error";
                logger.LogError(
                    "Script processing failed for Logsheet {LogsheetId}: {Error}",
                    logsheet.Id,
                    errorMessage
                );
                AdjustAfterFailedProcessing(logsheet, errorMessage);

                return outputResult.ToResult();
            }

            var output = outputResult.Value;

            var extractedData = output
                .ExtractedData.BuildAdapter()
                .AddParameters("LogsheetId", logsheet.Id)
                .AdaptToType<IEnumerable<ExtractedValue>>()
                .ToList();

            EvaluateValidationWarnings(logsheet, extractedData);

            logger.LogInformation(
                "Script processing successful for Logsheet {LogsheetId}. Extracted {Count} values.",
                logsheet.Id,
                extractedData.Count
            );

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

    public async Task<Result<LogsheetDetailDto>> ProcessLogsheetAsync(
        Logsheet logsheet,
        ProcessLogsheetDataOptions? options,
        CancellationToken ct
    )
    {
        var validationResult = ValidateLogsheetForProcessing(logsheet);
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

    private static void AdjustAfterSuccessfulProcessing(
        Logsheet logsheet,
        List<ExtractedValue> extractedData
    )
    {
        logsheet!.ExtractedValues.Clear();
        extractedData.ForEach(logsheet.ExtractedValues.Add);
        logsheet.ProcessedAt = DateTime.UtcNow;
        logsheet.Status = ELogSheetStatus.NeedsReview;
        logsheet.ErrorMessage = string.Empty;
    }

    private void AdjustAfterFailedProcessing(Logsheet logsheet, string errorMessage)
    {
        logsheet.Status = ELogSheetStatus.Failed;
        logsheet.ErrorMessage = errorMessage;
    }

    private void EvaluateValidationWarnings(Logsheet logsheet, List<ExtractedValue> extractedData)
    {
        var rulesVersion = roiValidationRuleCatalogProvider.GetCatalog().Version;
        var roisByIdFront =
            logsheet.Template?.Rois?.ToDictionary(r => r.Id) ?? new Dictionary<Guid, Roi>();
        var roisByIdBack =
            logsheet.Template?.BacksideTemplate?.Rois?.ToDictionary(r => r.Id)
            ?? new Dictionary<Guid, Roi>();
        var roisById = roisByIdFront
            .Concat(roisByIdBack)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var hasRoiContext = roisById.Count > 0;

        foreach (var extractedValue in extractedData)
        {
            extractedValue.ValidationRulesVersion = rulesVersion;

            if (!hasRoiContext)
            {
                extractedValue.ValidationWarnings =
                [
                    new RoiValidationWarningSnapshot(
                        "validation.roi.contextMissing",
                        "ROI context was not available during validation evaluation.",
                        "root"
                    ),
                ];
                continue;
            }

            if (!roisById.TryGetValue(extractedValue.RoiId, out var roi))
            {
                extractedValue.ValidationWarnings =
                [
                    new RoiValidationWarningSnapshot(
                        "validation.roi.notFound",
                        $"ROI '{extractedValue.RoiId}' was not found for validation.",
                        "root"
                    ),
                ];
                continue;
            }

            var warnings = roiValidationConditionEvaluator
                .Evaluate(roi.Type, extractedValue.Value, roi.ValidationCondition)
                .Select(w => new RoiValidationWarningSnapshot(w.Code, w.Message, w.Path))
                .ToList();

            extractedValue.ValidationWarnings = warnings;
        }
    }
}
