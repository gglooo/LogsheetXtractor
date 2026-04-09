using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Application.MessageProcessing;
using LogsheetXtractor.Domain.Enums;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace LogsheetXtractor.Application.Features.Logsheets;

public record AlignLogsheetCommand(Guid LogsheetId);

public static class AlignLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(
        AlignLogsheetCommand request,
        IAppDbContext dbContext,
        Envelope envelope,
        ILogsheetService logsheetService,
        IMessageBus bus,
        ILogger<AlignLogsheetCommand> logger,
        CancellationToken ct
    )
    {
        logger.LogInformation(
            "Starting automatic alignment for Logsheet {LogsheetId}",
            request.LogsheetId
        );

        var logsheet = await dbContext.Logsheets.FindAsync(request.LogsheetId, ct);
        if (logsheet is null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found for alignment", request.LogsheetId);
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        var shouldResetStatus = logsheet.Status == ELogSheetStatus.Aligning;

        Result<LogsheetDetailDto> result;
        string? errorMessage;

        try
        {
            result = await logsheetService.AlignLogsheetAsync(logsheet, ct);
            if (shouldResetStatus && result.IsSuccess)
            {
                logsheet.Status = ELogSheetStatus.Pending;
            }

            errorMessage = result.IsSuccess
                ? null
                : string.Join(", ", result.Errors.Select(e => e.Message).ToArray());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var retryPolicy = MessageRetryPolicies.For<AlignLogsheetCommand>();
            if (retryPolicy.IsRetryable(ex) && envelope.Attempts < retryPolicy.MaxAttempts)
            {
                throw;
            }

            errorMessage = $"Exception during automatic alignment: {ex.Message}";

            result = Result.Fail<LogsheetDetailDto>(errorMessage);
        }

        await bus.PublishAsync(
            new LogsheetAutomaticAlignmentFinished(
                request.LogsheetId,
                errorMessage == null,
                errorMessage
            )
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        logger.LogInformation(
            "Finished automatic alignment for Logsheet {LogsheetId} with result: {Result}",
            request.LogsheetId,
            result.IsSuccess ? "Success" : $"Failed with errors: {errorMessage}"
        );

        return result;
    }
}
