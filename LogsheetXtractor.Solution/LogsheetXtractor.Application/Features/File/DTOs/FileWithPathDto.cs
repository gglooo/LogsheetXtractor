namespace LogsheetXtractor.Application.Features.File.DTOs;

/// <summary>
/// File metadata together with the filesystem path that identifies the file on disk.
/// <param name="Id">The file identifier.</param>
/// <param name="FileName">The original file name.</param>
/// <param name="FilePath">The filesystem path used to locate the file on disk.</param>
/// <param name="ContentType">The file MIME type.</param>
/// <param name="SizeBytes">The file size in bytes.</param>
/// <param name="CreatedAt">The date and time when the file record was created.</param>
/// </summary>
public record FileWithPathDto(
    Guid Id,
    string FileName,
    string FilePath,
    string ContentType,
    uint SizeBytes,
    DateTime CreatedAt
);
