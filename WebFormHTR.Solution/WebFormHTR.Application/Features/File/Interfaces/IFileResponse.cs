namespace WebFormHTR.Application.Features.File.Interfaces;

public interface IFileResponse
{
    Stream Stream { get; }
    string ContentType { get; }
    string? FileName { get; }
}