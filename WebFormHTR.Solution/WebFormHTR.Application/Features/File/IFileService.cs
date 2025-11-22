using WebFormHTR.Application.DTOs;

namespace WebFormHTR.Application.Features.File;

public interface IFileService
{
    public Task<Domain.Entities.File> UploadFileAsync(byte[] fileContent, string fileName, string contentType);
    public Task<GetFileDto?> GetFileAsync(Guid id);
}