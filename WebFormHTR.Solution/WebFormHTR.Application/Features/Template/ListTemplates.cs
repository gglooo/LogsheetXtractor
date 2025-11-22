using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record ListTemplatesQuery(string? Search);

public static class ListTemplatesHandler
{
    public static Task<IEnumerable<Domain.Entities.Template>> Handle(ListTemplatesQuery request, IAppDbContext dbContext)
    {
        var query = dbContext.Templates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.Name.Contains(request.Search));
        }

        return Task.FromResult<IEnumerable<Domain.Entities.Template>>(query.ToList());
    }
}