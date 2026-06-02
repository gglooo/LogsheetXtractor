using LogsheetXtractor.Application.Features.File.Interfaces;

namespace LogsheetXtractor.Application.DTOs;

/// <summary>
/// TODO-DOC: Describe GetFileDto purpose and usage.
/// TODO-DOC-MEMBERS: Document public properties.
/// </summary>
public class GetFileDto : IFileResponse
{
    public Stream? Stream { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? FileName { get; set; }
}
