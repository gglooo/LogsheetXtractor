using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.Scripting.DTOs;

public record ProcessLogsheetInputDto(Logsheet Logsheet);

public record ProcessLogsheetOutputDto(Dictionary<string, (string, bool)> ExtractedData);