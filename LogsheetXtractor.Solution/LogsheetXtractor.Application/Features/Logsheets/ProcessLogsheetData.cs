using LogsheetXtractor.Application.Extensions;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Application.MessageProcessing;
using LogsheetXtractor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace LogsheetXtractor.Application.Features.Logsheets;

public sealed record ProcessLogsheetDataOptions(bool? UglyCheckboxes);

public sealed record ProcessLogsheetDataCommand(
    Guid LogsheetId,
    ProcessLogsheetDataOptions? Options
);

public static class ProcessLogsheetDataHandler
{
    public static async Task Handle(
        ProcessLogsheetDataCommand request,
        IAppDbContext dbContext,
        Envelope envelope,
        IMessageBus bus,
        ILogsheetService logsheetService,
        ICredentialCookieAccessor credentialCookieAccessor,
        CancellationToken ct
    )
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(
            ls => ls.Id == request.LogsheetId,
            ct
        );
        if (logsheet is null)
        {
            await bus.PublishWithContextAsync(
                new LogsheetProcessingFinishedEvent(
                    request.LogsheetId,
                    false,
                    "Logsheet not found"
                ),
                credentialCookieAccessor
            );
            await dbContext.SaveChangesAsync(ct);
            return;
        }

        try
        {
            var processResult = await logsheetService.ProcessLogsheetAsync(
                logsheet,
                request.Options,
                ct
            );
            string? errorMsg = null;
            if (processResult.IsFailed)
            {
                errorMsg = string.Join(", ", processResult.Errors.Select(e => e.Message));
            }

            await bus.PublishWithContextAsync(
                new LogsheetProcessingFinishedEvent(
                    request.LogsheetId,
                    processResult.IsSuccess,
                    errorMsg
                ),
                credentialCookieAccessor
            );

            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var retryPolicy = MessageRetryPolicies.For<ProcessLogsheetDataCommand>();
            if (retryPolicy.IsRetryable(ex) && envelope.Attempts < retryPolicy.MaxAttempts)
            {
                throw;
            }

            dbContext.ChangeTracker.Clear();

            var failedLogsheet = await dbContext.Logsheets.FirstOrDefaultAsync(
                ls => ls.Id == request.LogsheetId,
                CancellationToken.None
            );
            var errorMessage = $"Exception during processing: {ex.Message}";
            if (failedLogsheet is not null)
            {
                failedLogsheet.Status = ELogSheetStatus.Failed;
                failedLogsheet.ErrorMessage = errorMessage;
            }

            await bus.PublishWithContextAsync(
                new LogsheetProcessingFinishedEvent(
                    request.LogsheetId,
                    false,
                    errorMessage
                ),
                credentialCookieAccessor
            );

            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
    }
}
