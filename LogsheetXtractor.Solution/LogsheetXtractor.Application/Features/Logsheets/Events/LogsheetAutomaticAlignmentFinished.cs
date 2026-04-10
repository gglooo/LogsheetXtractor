namespace LogsheetXtractor.Application.Features.Logsheets.Events;

public record LogsheetAutomaticAlignmentFinished(Guid LogsheetId, bool IsSuccess, string? ErrorMessage);
