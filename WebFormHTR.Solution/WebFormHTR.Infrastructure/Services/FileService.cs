using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Services;

public class FileService(IAppDbContext dbContext, IMapper mapper, IFileStorageService fileStorageService) : IFileService
{
    public async Task<FileDto> UploadFileAsync(byte[] fileContent, string fileName, string contentType)
    {
        var storagePath = await fileStorageService.SaveFileAsync(fileContent, fileName);

        var file = new Domain.Entities.File
        {
            ContentType = contentType,
            OriginalFileName = fileName,
            StoredFileName = $"{Guid.NewGuid()}_{fileName}",
            StoragePath = storagePath,
            SizeBytes = (uint)fileContent.Length
        };

        dbContext.Files.Add(file);

        return mapper.Map<FileDto>(file);
    }

    public async Task<GetFileDto?> GetFileAsync(Guid id)
    {
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == id);

        if (file is null)
        {
            return null;
        }

        var filePath = file.StoragePath;
        try
        {
            var stream = fileStorageService.GetFile(filePath);

            return new GetFileDto { Stream = stream, ContentType = file.ContentType, FileName = file.OriginalFileName };
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    public async Task<FileDto> CloneFileAsync(Guid fileId)
    {
        var file = await dbContext.Files.FirstAsync(f => f.Id == fileId);

        var fileBytes = await fileStorageService.ReadFileAsync(file.StoragePath);
        return await UploadFileAsync(fileBytes, file.OriginalFileName, file.ContentType);
    }
}