using FluentResults;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Template;

public sealed record ListTemplatesQuery(string? Search);

public static class ListTemplatesHandler
{
    public static async Task<Result<IEnumerable<TemplateListDto>>> Handle(
        ListTemplatesQuery request,
        IAppDbContext dbContext
    )
    {
        var query = dbContext.Templates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.Name.Contains(request.Search));
        }

        var templates = await query
            .Where(t => t.FrontsideTemplate == null)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TemplateListDto(
                t.Id,
                t.Name,
                t.BacksideTemplateId,
                t.ParentId,
                t.FileId,
                t.Rois.Count() + (t.BacksideTemplate != null ? t.BacksideTemplate.Rois.Count() : 0),
                t.Logsheets.Count(),
                t.Width ?? 0,
                t.Height ?? 0,
                t.CreatedAt
            ))
            .ToListAsync();

        return Result.Ok(templates.AsEnumerable());
    }
}
