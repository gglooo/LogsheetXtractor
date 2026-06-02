namespace LogsheetXtractor.Application.Features.File.DTOs;

/// <summary>
/// TODO-DOC: Describe FileWithPathDto purpose and usage.
/// <param name="Id">TODO-DOC: Describe Id.</param>
/// <param name="FileName">TODO-DOC: Describe FileName.</param>
/// <param name="FilePath">TODO-DOC: Describe FilePath.</param>
/// <param name="ContentType">TODO-DOC: Describe ContentType.</param>
/// <param name="SizeBytes">TODO-DOC: Describe SizeBytes.</param>
/// <param name="CreatedAt">TODO-DOC: Describe CreatedAt.</param>
/// </summary>
public record FileWithPathDto(
    Guid Id,
    string FileName,
    string FilePath,
    string ContentType,
    uint SizeBytes,
    DateTime CreatedAt
);
