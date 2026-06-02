namespace LogsheetXtractor.Application.Features.Template.DTOs;

/// <summary>
/// Minimal template reference used for parent/front/back relations.
/// </summary>
public record TemplateReferenceDto(Guid Id, string Name, int Width, int Height, Guid FileId);
