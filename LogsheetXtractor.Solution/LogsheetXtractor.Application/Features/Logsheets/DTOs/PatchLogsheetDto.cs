namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

/// <summary>
/// Alignment data that may be changed on a logsheet.
/// <param name="AlignmentData">The alignment data to store for the logsheet.</param>
/// </summary>
public record PatchLogsheetDto(AlignmentDataDto? AlignmentData);
