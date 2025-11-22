using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.File;

public sealed record GetFileQuery(string Id);

public static class GetFileHandler
{
    public static async Task<GetFileDto?> Handle(GetFileQuery request, IFileService fileService)
    {
        return await fileService.GetFileAsync(Guid.Parse(request.Id));
    }
}