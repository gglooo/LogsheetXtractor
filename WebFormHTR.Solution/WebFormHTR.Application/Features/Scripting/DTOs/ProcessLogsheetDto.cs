namespace WebFormHTR.Application.Features.Scripting.DTOs;

public record ProcessLogsheetInputDto(string FilePath);

public record ProcessLogsheetOutputDto(Dictionary<string, string> ExtractedData);