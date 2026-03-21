using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Template;

public sealed record GetTemplatePreviewQuery(Guid TemplateId);

public static class GetTemplatePreviewHandler
{
    public static async Task<Result<GetFileDto>> HandleAsync(
        GetTemplatePreviewQuery query,
        IAppDbContext dbContext,
        IFileService fileService,
        CancellationToken ct
    )
    {
        try
        {
            var template = await dbContext
                .Templates.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == query.TemplateId, ct);

            if (template is null)
            {
                return Result.Fail<GetFileDto>(new NotFoundError("Template not found"));
            }

            var fileDto = await fileService.GetFilePreviewAsync(template.FileId);
            if (fileDto is null)
            {
                return Result.Fail<GetFileDto>(
                    new NotFoundError("Template preview not available.")
                );
            }

            return Result.Ok(fileDto);
        }
        catch (Exception e)
        {
            return Result.Fail<GetFileDto>(e.Message);
        }
    }
}
