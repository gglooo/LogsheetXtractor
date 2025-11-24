using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Infrastructure.Services;

public class FileService(IAppDbContext dbContext, IMapper mapper) : IFileService
{
    private readonly string _storageDirectory = "FileStorage";
    public async Task<FileDto> UploadFileAsync(byte[] fileContent, string fileName, string contentType)
    {
        var newFileName = Guid.NewGuid().ToString();
        var storagePath = Path.Combine(_storageDirectory, newFileName);
        var file = new Domain.Entities.File()
        {
            ContentType = contentType,
            OriginalFileName = fileName,
            StoredFileName = $"{Guid.NewGuid()}_{fileName}",
            StoragePath = storagePath,
            SizeBytes = (uint)fileContent.Length
        };

        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }

        await File.WriteAllBytesAsync(storagePath, fileContent);

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
        // TODO: add security check if path is within scope
        if (!File.Exists(filePath))
        {
            return null;
        }

        var stream = new FileStream(
            file.StoragePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return new GetFileDto() { Stream = stream, ContentType = file.ContentType, FileName = file.OriginalFileName };
    }

    public async Task<FileDto> CloneFileAsync(Guid fileId)
    {
        var file = await dbContext.Files.FirstAsync(f => f.Id == fileId);

        var fileBytes = await File.ReadAllBytesAsync(file.StoragePath);
        return await UploadFileAsync(fileBytes, file.OriginalFileName, file.ContentType);
    }
}