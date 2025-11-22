using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.File;

public sealed record UploadFileCommand(byte[] FileContent, string FileName, string ContentType);

public static class UploadFileHandler
{
    public static async Task<Domain.Entities.File> Handle(UploadFileCommand request, IFileService fileService)
    {
       return await fileService.UploadFileAsync(request.FileContent, request.FileName, request.ContentType);
    }
}