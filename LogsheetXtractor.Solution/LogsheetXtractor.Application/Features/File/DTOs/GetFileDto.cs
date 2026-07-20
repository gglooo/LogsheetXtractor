using LogsheetXtractor.Application.Features.File.Interfaces;

namespace LogsheetXtractor.Application.DTOs;

/// <summary>
/// File content and download metadata returned when an application service reads a file.
/// </summary>
public class GetFileDto : IFileResponse
{
    /// <summary>The readable file content stream, when the file was opened successfully.</summary>
    public Stream? Stream { get; set; }
    /// <summary>The MIME type sent with the file response.</summary>
    public string ContentType { get; set; } = string.Empty;
    /// <summary>The suggested file name for the response, when available.</summary>
    public string? FileName { get; set; }
}
