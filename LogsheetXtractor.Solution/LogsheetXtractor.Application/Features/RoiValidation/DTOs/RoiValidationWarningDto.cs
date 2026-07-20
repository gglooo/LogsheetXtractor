namespace LogsheetXtractor.Application.Features.RoiValidation.DTOs;

/// <summary>
/// A validation warning produced while evaluating an ROI condition.
/// <param name="Code">The stable warning code.</param>
/// <param name="Message">The human-readable warning message.</param>
/// <param name="Path">The path to the condition node that produced the warning.</param>
/// </summary>
public sealed record RoiValidationWarningDto(string Code, string Message, string Path);
