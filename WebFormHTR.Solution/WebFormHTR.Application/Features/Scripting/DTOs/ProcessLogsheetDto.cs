using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.Scripting.DTOs;

public record ProcessLogsheetInputOptionsDto(bool? UglyCheckboxes);

public record ProcessLogsheetInputDto(Logsheet Logsheet, ProcessLogsheetInputOptionsDto? Options);

public record ProcessLogsheetOutputDto(Dictionary<string, string> ExtractedData);