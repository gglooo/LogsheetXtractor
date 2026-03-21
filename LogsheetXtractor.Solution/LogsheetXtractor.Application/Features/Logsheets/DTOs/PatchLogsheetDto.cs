using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

// TODO: add other fields that can be patched
public record PatchLogsheetDto(string? FrontAlignmentData, string? BackAlignmentData);
