using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

// TODO: add other fields that can be patched
/// <summary>
/// TODO-DOC: Describe PatchLogsheetDto purpose and usage.
/// <param name="FrontAlignmentData">TODO-DOC: Describe FrontAlignmentData.</param>
/// <param name="BackAlignmentData">TODO-DOC: Describe BackAlignmentData.</param>
/// </summary>
public record PatchLogsheetDto(string? FrontAlignmentData, string? BackAlignmentData);
