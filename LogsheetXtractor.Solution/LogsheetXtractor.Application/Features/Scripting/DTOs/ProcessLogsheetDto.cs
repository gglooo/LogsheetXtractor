using LogsheetXtractor.Domain.Entities;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

/// <summary>
/// Optional flags forwarded to the logsheet-processing script.
/// <param name="UglyCheckboxes">Whether to enable script handling for checkboxes with irregular shapes or large edges.</param>
/// </summary>
public record ProcessLogsheetInputOptionsDto(bool? UglyCheckboxes);

/// <summary>
/// Input passed to the logsheet-processing script.
/// <param name="Logsheet">The logsheet to process.</param>
/// <param name="Options">Optional processing flags.</param>
/// </summary>
public record ProcessLogsheetInputDto(Logsheet Logsheet, ProcessLogsheetInputOptionsDto? Options);

/// <summary>
/// Result returned by the logsheet-processing script.
/// <param name="ExtractedData">The extracted values keyed by ROI variable name.</param>
/// </summary>
public record ProcessLogsheetOutputDto(Dictionary<string, string> ExtractedData);
