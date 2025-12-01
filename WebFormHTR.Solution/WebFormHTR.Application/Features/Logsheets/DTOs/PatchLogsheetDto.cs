using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets.DTOs;

// TODO: add other fields that can be patched
public record PatchLogsheetDto(
    string? FrontAlignmentData,
    string? BackAlignmentData
);