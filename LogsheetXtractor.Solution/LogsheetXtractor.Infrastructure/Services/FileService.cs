using System.Runtime.InteropServices;
using Docnet.Core;
using Docnet.Core.Models;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.File;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Services.Storage;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace LogsheetXtractor.Infrastructure.Services;

public class FileService(
    IAppDbContext dbContext,
    IMapper mapper,
    IFileStorageService fileStorageService,
    IDocLib docLib,
    ILogger<FileService> logger,
    IMemoryCache memoryCache
) : IFileService
{
    public async Task<FileDto> UploadFileAsync(
        byte[] fileContent,
        string fileName,
        string contentType
    )
    {
        logger.LogInformation(
            "Uploading file: {FileName}, ContentType: {ContentType}, Size: {Size}",
            fileName,
            contentType,
            fileContent.Length
        );
        var storagePath = await fileStorageService.SaveFileAsync(fileContent, fileName);

        var file = new LogsheetXtractor.Domain.Entities.File
        {
            ContentType = contentType,
            OriginalFileName = fileName,
            StoredFileName = $"{Guid.NewGuid()}_{fileName}",
            StoragePath = storagePath,
            SizeBytes = (uint)fileContent.Length,
        };

        dbContext.Files.Add(file);

        return mapper.Map<FileDto>(file);
    }

    public async Task<GetFileDto?> GetFileAsync(Guid id)
    {
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == id);

        if (file is null)
        {
            logger.LogWarning("File not found in database. Id: {FileId}", id);
            return null;
        }

        var filePath = file.StoragePath;
        try
        {
            var stream = fileStorageService.GetFile(filePath);

            return new GetFileDto
            {
                Stream = stream,
                ContentType = file.ContentType,
                FileName = file.OriginalFileName,
            };
        }
        catch (FileNotFoundException)
        {
            logger.LogError(
                "File not found in storage. Path: {StoragePath}, FileId: {FileId}",
                filePath,
                id
            );
            return null;
        }
    }

    public async Task<FileDto> CloneFileAsync(Guid fileId)
    {
        logger.LogInformation("Cloning file {FileId}", fileId);
        var file = await dbContext.Files.FirstAsync(f => f.Id == fileId);

        var fileBytes = await fileStorageService.ReadFileAsync(file.StoragePath);
        return await UploadFileAsync(fileBytes, file.OriginalFileName, file.ContentType);
    }

    private void DeleteFileFromStorage(LogsheetXtractor.Domain.Entities.File file)
    {
        try
        {
            fileStorageService.DeleteFile(file.StoragePath);
            logger.LogInformation(
                "Deleted file from storage. StoragePath: {StoragePath}, FileId: {FileId}",
                file.StoragePath,
                file.Id
            );
        }
        catch (FileNotFoundException)
        {
            logger.LogError(
                "File not found in storage during deletion. StoragePath: {StoragePath}, FileId: {FileId}",
                file.StoragePath,
                file.Id
            );
        }
    }

    public async Task DeleteFileAsync(Guid id)
    {
        logger.LogInformation("Deleting file {FileId}", id);
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == id);
        if (file is null)
        {
            logger.LogWarning("Attempted to delete non-existent file. FileId: {FileId}", id);
            return;
        }

        DeleteFileFromStorage(file);

        dbContext.Files.Remove(file);
    }

    public Task DeleteFilesAsync(IEnumerable<Guid> ids)
    {
        var idsList = ids.ToList();
        logger.LogInformation(
            "Deleting multiple files. FileIds: {FileIds}",
            string.Join(", ", idsList)
        );
        var files = dbContext.Files.Where(f => idsList.Contains(f.Id)).ToList();

        foreach (var file in files)
        {
            DeleteFileFromStorage(file);
        }

        dbContext.Files.RemoveRange(files);
        return Task.CompletedTask;
    }

    public async Task<GetFileDto?> GetFileFromContentAsync(
        byte[] content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Saving temporary file {FileName} from content (Size: {Size})",
            fileName,
            content.Length
        );
        var tempFilePath = await fileStorageService.SaveTemporaryFileAsync(
            content,
            fileName,
            cancellationToken
        );
        var stream = fileStorageService.GetFile(tempFilePath);

        return new GetFileDto
        {
            Stream = stream,
            ContentType = contentType,
            FileName = fileName,
        };
    }

    public async Task<GetFileDto?> ConvertToImageAsync(Guid fileId)
    {
        logger.LogInformation("Converting file to image. FileId: {FileId}", fileId);
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId);
        if (file is null || file.ContentType != "application/pdf")
        {
            logger.LogWarning("File not found or not a PDF. FileId: {FileId}", fileId);
            return null;
        }

        var fileContent = await fileStorageService.ReadFileAsync(file.StoragePath);

        using var docReader = docLib.GetDocReader(fileContent, new PageDimensions(2.0));
        // NOTE: only supporting first page, as this only gets used for templates
        using var pageReader = docReader.GetPageReader(0);

        var rawBytes = pageReader.GetImage();
        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();

        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var bitmap = new SKBitmap(info);

        var pixelsAddr = bitmap.GetPixels();
        Marshal.Copy(rawBytes, 0, pixelsAddr, rawBytes.Length);

        var destInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var destSurface = SKSurface.Create(destInfo);
        var canvas = destSurface.Canvas;

        // White background to replace transparency
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(bitmap, 0, 0);

        using var image = destSurface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        var encodedStream = new MemoryStream();
        data.SaveTo(encodedStream);
        encodedStream.Position = 0;

        logger.LogInformation("Successfully converted file to image. FileId: {FileId}", fileId);

        return new GetFileDto
        {
            Stream = encodedStream,
            ContentType = "image/png",
            FileName = Path.ChangeExtension(file.OriginalFileName, ".png"),
        };
    }

    public async Task<GetFileDto?> GetFilePreviewAsync(Guid fileId)
    {
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId);
        if (file is null || file.ContentType != "application/pdf")
        {
            return null;
        }

        var cacheKey = $"template-preview-{fileId}";
        if (!memoryCache.TryGetValue(cacheKey, out byte[]? cachedImage))
        {
            logger.LogInformation("Generating preview for file. FileId: {FileId}", fileId);
            var fileContent = await fileStorageService.ReadFileAsync(file.StoragePath);

            using var docReader = docLib.GetDocReader(fileContent, new PageDimensions(0.7));
            using var pageReader = docReader.GetPageReader(0);

            var rawBytes = pageReader.GetImage();
            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var bitmap = new SKBitmap(info);

            var pixelsAddr = bitmap.GetPixels();
            Marshal.Copy(rawBytes, 0, pixelsAddr, rawBytes.Length);

            var destInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
            using var destSurface = SKSurface.Create(destInfo);
            var canvas = destSurface.Canvas;

            // White background to replace transparency
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(bitmap, 0, 0);

            using var image = destSurface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Webp, 50);

            using var memoryStream = new MemoryStream();
            data.SaveTo(memoryStream);
            cachedImage = memoryStream.ToArray();

            var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                TimeSpan.FromDays(1)
            );

            memoryCache.Set(cacheKey, cachedImage, cacheOptions);
            logger.LogInformation("Preview cached for file. FileId: {FileId}", fileId);
        }
        else
        {
            logger.LogInformation("Serving preview from cache. FileId: {FileId}", fileId);
        }

        return new GetFileDto
        {
            Stream = new MemoryStream(cachedImage!),
            ContentType = "image/webp",
            FileName = Path.ChangeExtension(file.OriginalFileName, "_preview.webp"),
        };
    }
}
