namespace LogsheetXtractor.Application.Features.ExtractedValues.DTOs;

/// <summary>
/// Input for verifying an extracted value and optionally recording a correction.
/// <param name="CorrectedValue">The corrected text to store; null or empty leaves the extracted value uncorrected.</param>
/// </summary>
public record VerifyExtractedValueDto(
    string? CorrectedValue
);
