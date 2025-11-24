using FluentResults;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.File;

public sealed record UploadFileCommand(byte[] FileContent, string FileName, string ContentType);

public static class UploadFileHandler
{
    public static async Task<Result<FileDto>> Handle(UploadFileCommand request, IFileService fileService, IAppDbContext dbContext, CancellationToken ct)
    {
        try
        {
            var res = await fileService.UploadFileAsync(request.FileContent, request.FileName, request.ContentType);

            await dbContext.SaveChangesAsync(ct);

            return Result.Ok(res);
        }
        catch (Exception ex)
        {
            return Result.Fail<FileDto>(ex.Message);
        }
    }
}