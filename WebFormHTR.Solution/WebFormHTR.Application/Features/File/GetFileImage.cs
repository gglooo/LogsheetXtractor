using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.File;

public sealed record GetFileImageQuery(Guid FileId);

public static class GetFileImageHandler
{
    public static async Task<Result<GetFileDto>> HandleAsync(
        GetFileImageQuery query,
        IAppDbContext dbContext,
        IFileService fileService,
        CancellationToken ct
    )
    {
        try
        {
            var file = await dbContext.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == query.FileId, ct);

            if (file is null)
            {
                return Result.Fail<GetFileDto>(new NotFoundError("File not found"));
            }

            var fileDto = await fileService.ConvertToImageAsync(query.FileId);
            if (fileDto is null)
            {
                return Result.Fail<GetFileDto>(new ConstraintError("Failed to convert file to image"));
            }

            return Result.Ok(fileDto);
        }
        catch (Exception e)
        {
            return Result.Fail<GetFileDto>(e.Message);
        }
    }
}