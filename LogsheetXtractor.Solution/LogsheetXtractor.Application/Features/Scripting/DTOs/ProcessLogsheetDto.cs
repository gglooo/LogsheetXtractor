using LogsheetXtractor.Domain.Entities;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

/// <summary>
/// TODO-DOC: Describe ProcessLogsheetInputOptionsDto purpose and usage.
/// <param name="Options">TODO-DOC: Describe Options.</param>
/// </summary>
public record ProcessLogsheetInputOptionsDto(bool? UglyCheckboxes);

/// <summary>
/// TODO-DOC: Describe ProcessLogsheetInputDto purpose and usage.
/// <param name="Logsheet">TODO-DOC: Describe Logsheet.</param>
/// <param name="ExtractedData">TODO-DOC: Describe ExtractedData.</param>
/// </summary>
public record ProcessLogsheetInputDto(Logsheet Logsheet, ProcessLogsheetInputOptionsDto? Options);

/// <summary>
/// TODO-DOC: Describe ProcessLogsheetOutputDto purpose and usage.
/// <param name="ExtractedData">TODO-DOC: Describe ExtractedData.</param>
/// </summary>
public record ProcessLogsheetOutputDto(Dictionary<string, string> ExtractedData);
