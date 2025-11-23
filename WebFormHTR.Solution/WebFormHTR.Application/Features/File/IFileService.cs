using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;

namespace WebFormHTR.Application.Features.File;

public interface IFileService
{
    public Task<FileDto> UploadFileAsync(byte[] fileContent, string fileName, string contentType);
    public Task<GetFileDto?> GetFileAsync(Guid id);
}