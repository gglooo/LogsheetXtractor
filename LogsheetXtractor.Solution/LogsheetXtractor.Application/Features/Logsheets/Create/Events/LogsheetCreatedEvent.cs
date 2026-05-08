namespace LogsheetXtractor.Application.Features.Logsheets.Create.Events;

public record LogsheetCreatedEvent(Guid LogsheetId, bool PerformAutomaticAlignment = true);
