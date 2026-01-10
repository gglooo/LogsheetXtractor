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

namespace WebFormHTR.Infrastructure.Services;

public class LogsheetService(
    IMapper mapper,
    IHtrScriptEngine scriptEngine
) : ILogsheetService
{
    public Error? ValidateLogsheet(Logsheet? logsheet)
    {
        if (logsheet is null)
        {
            return new NotFoundError("Logsheet not found");
        }

        if (logsheet.Status != ELogSheetStatus.Pending || logsheet.ProcessedAt is not null)
        {
            return new InvalidStateError("Logsheet is not in a valid state for processing");
        }

        return null;
    }

    private async Task InvokeLogsheetProcessing(Logsheet logsheet, CancellationToken ct)
    {
        try
        {
            var output = await scriptEngine.ProcessLogsheetAsync(new ProcessLogsheetInputDto(logsheet), ct);

            var extractedData = output.ExtractedData.BuildAdapter()
                .AddParameters("LogsheetId", logsheet.Id)
                .AdaptToType<IEnumerable<ExtractedValue>>()
                .ToList();

            AdjustAfterSuccessfulProcessing(logsheet, extractedData);
        }
        catch (Exception ex)
        {
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
        var processedLogsheets = new List<LogsheetDetailDto>();
        foreach (var logsheet in logsheets)
        {
            try
            {
                var processedLogsheet = await ProcessLogsheetAsync(logsheet, ct);
                processedLogsheets.Add(processedLogsheet);
            }
            catch (ValidationException ex)
            {
                // TODO: do something better here
                Console.WriteLine($"Validation error for Logsheet ID {logsheet.Id}: {ex.Message}");
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