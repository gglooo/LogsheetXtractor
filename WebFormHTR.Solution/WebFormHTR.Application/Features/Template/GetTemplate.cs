using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record GetTemplateQuery(string Id);

public static class GetTemplateHandler
{
    public static Task<Domain.Entities.Template?> Handle(GetTemplateQuery request, IAppDbContext dbContext)
    {
        return Task.FromResult(dbContext.Templates.FirstOrDefault(t => t.Id.ToString() == request.Id));
    }
}