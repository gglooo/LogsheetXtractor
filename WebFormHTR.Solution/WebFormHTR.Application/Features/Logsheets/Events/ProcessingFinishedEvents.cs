namespace WebFormHTR.Application.Features.Logsheets.Events;

public record LogsheetProcessingFinishedEvent(Guid LogsheetId, bool IsSuccess, string? ErrorMessage);

public record BatchProcessingFinishedEvent(Guid[] ProcessedLogsheetIds, Guid[] FailedLogsheetIds, List<string> ErrorMessages);
