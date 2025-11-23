using FluentResults;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.File;

public sealed record GetFileQuery(string Id);

public static class GetFileHandler
{
    public static async Task<Result<GetFileDto?>> Handle(GetFileQuery request, IFileService fileService)
    {
        try {
            var res = await fileService.GetFileAsync(Guid.Parse(request.Id));
            return Result.Ok(res);
        }
        catch (Exception ex)
        {
            return Result.Fail<GetFileDto?>(ex.Message);
        }
    }
}