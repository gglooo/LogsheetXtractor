using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record GetTemplateQuery(Guid Id);

public static class GetTemplateHandler
{
    public static Task<Result<TemplateDetailDto?>> Handle(GetTemplateQuery request, IAppDbContext dbContext,
        IMapper mapper)
    {
        var template = dbContext.Templates
            .AsNoTracking()
            .Include(t => t.Rois)
            .Include(t => t.BacksideTemplate)
            .Include(t => t.FrontsideTemplate)
            .Include(t => t.File)
            .FirstOrDefault(t => t.Id == request.Id);

        var result = template is null ? null : mapper.Map<TemplateDetailDto>(template);

        return Task.FromResult(Result.Ok(result));
    }
}