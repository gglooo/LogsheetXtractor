namespace WebFormHTR.Application.Features.Logsheets.Events;

public record LogsheetProcessingFinishedEvent(Guid LogsheetId, bool IsSuccess, string? ErrorMessage);
