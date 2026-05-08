using System.Net.Mime;
using LogsheetXtractor.Domain.Entities.Base;

namespace LogsheetXtractor.Domain.Entities;

public class File : BaseEntity
{
    public string OriginalFileName { get; set; } = String.Empty;
    public string StoredFileName { get; set; } = String.Empty;
    public string StoragePath { get; set; } = String.Empty;
    public string ContentType { get; set; } = MediaTypeNames.Application.Octet;
    public uint SizeBytes { get; set; }
}