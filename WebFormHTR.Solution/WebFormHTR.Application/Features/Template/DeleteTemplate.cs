using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record DeleteTemplateCommand(Guid Id);

public static class DeleteTemplateHandler
{
    public static async Task<Result> Handle(DeleteTemplateCommand request, IAppDbContext dbContext, CancellationToken ct)
    {
        var template = dbContext.Templates.FirstOrDefault(t => t.Id == request.Id);
        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        dbContext.Templates.Remove(template);
        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}