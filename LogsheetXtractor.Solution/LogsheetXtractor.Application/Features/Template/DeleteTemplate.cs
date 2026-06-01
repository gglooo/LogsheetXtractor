using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Template;

public sealed record DeleteTemplateCommand(Guid Id);

public static class DeleteTemplateHandler
{
    public static async Task<Result> Handle(
        DeleteTemplateCommand request,
        IAppDbContext dbContext,
        IFileService fileService,
        CancellationToken ct
    )
    {
        var template = await dbContext
            .Templates.Include(t => t.BacksideTemplate)
            .Include(t => t.FrontsideTemplate)
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct);
        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        var templatesToDelete = new List<LogsheetXtractor.Domain.Entities.Template> { template };

        if (template.BacksideTemplate is not null)
        {
            templatesToDelete.Add(template.BacksideTemplate);
        }
        else if (template.FrontsideTemplate is not null)
        {
            templatesToDelete.Add(template.FrontsideTemplate);
        }

        var fileIdsToDelete = templatesToDelete.Select(t => t.FileId).Distinct().ToList();

        dbContext.Templates.RemoveRange(templatesToDelete);
        await fileService.DeleteFilesAsync(fileIdsToDelete);

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
