namespace LogsheetXtractor.Application.Features.RoiValidation.DTOs;

/// <summary>
/// TODO-DOC: Describe RoiValidationWarningDto purpose and usage.
/// <param name="Code">TODO-DOC: Describe Code.</param>
/// <param name="Message">TODO-DOC: Describe Message.</param>
/// <param name="Path">TODO-DOC: Describe Path.</param>
/// </summary>
public sealed record RoiValidationWarningDto(string Code, string Message, string Path);
