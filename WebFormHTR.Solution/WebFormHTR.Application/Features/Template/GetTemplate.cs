using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record GetTemplateQuery(Guid Id);

public static class GetTemplateHandler
{
    public static async Task<Result<TemplateDetailDto?>> Handle(GetTemplateQuery request, IAppDbContext dbContext,
        IMapper mapper)
    {
        var template = await dbContext.Templates
            .AsNoTracking()
            .Include(t => t.Rois)
            .Include(t => t.BacksideTemplate)
            .Include(t => t.FrontsideTemplate)
            .ThenInclude(t => t.Logsheets)
            .Include(t => t.File)
            .Include(t => t.Logsheets)
            .FirstOrDefaultAsync(t => t.Id == request.Id, default);

        if (template is null)
        {
            return Result.Ok<TemplateDetailDto?>(null);
        }

        var isEditable = TemplateRules.IsEditable.Compile().Invoke(template);

        var dto = mapper.Map<TemplateDetailDto>(template) with
        {
            IsEditable = isEditable
        };

        return Result.Ok<TemplateDetailDto?>(dto);
    }
}