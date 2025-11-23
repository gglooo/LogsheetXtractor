namespace WebFormHTR.Application.DTOs;

public class GetFileDto
{
    public Stream? Stream { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? FileName { get; set; }
}