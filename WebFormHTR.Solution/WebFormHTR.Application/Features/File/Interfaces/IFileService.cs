using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;

namespace WebFormHTR.Application.Features.File.Interfaces;

public interface IFileService
{
    Task<FileDto> UploadFileAsync(byte[] fileContent, string fileName, string contentType);
    Task<GetFileDto?> GetFileAsync(Guid id);
    Task<FileDto> CloneFileAsync(Guid fileId);
    Task DeleteFileAsync(Guid id);
    Task DeleteFilesAsync(IEnumerable<Guid> ids);

    Task<GetFileDto?> GetFileFromContentAsync(byte[] content, string fileName, string contentType,
        CancellationToken cancellationToken);

    Task<GetFileDto?> ConvertToImageAsync(Guid fileId);

    Task<GetFileDto?> GetFilePreviewAsync(Guid fileId);
}