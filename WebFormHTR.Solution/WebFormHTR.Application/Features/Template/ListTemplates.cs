using FluentResults;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record ListTemplatesQuery(string? Search);

public static class ListTemplatesHandler
{
    public static Task<Result<IEnumerable<TemplateListDto>>> Handle(ListTemplatesQuery request, IAppDbContext dbContext,
        IMapper mapper)
    {
        var query = dbContext.Templates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.Name.Contains(request.Search));
        }

        var templates = query
            .Where(t => t.FrontsideTemplate == null)
            .Include(t => t.Parent)
            .Include(t => t.File)
            .Include(t => t.Rois)
            .OrderByDescending(t => t.CreatedAt)
            .AsNoTracking().ToList();

        return Task.FromResult(Result.Ok(mapper.Map<IEnumerable<TemplateListDto>>(templates)));
    }
}