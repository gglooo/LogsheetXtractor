using WebFormHTR.Application.Features.File.Interfaces;

namespace WebFormHTR.Application.DTOs;

public class GetFileDto : IFileResponse
{
    public Stream? Stream { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? FileName { get; set; }
}