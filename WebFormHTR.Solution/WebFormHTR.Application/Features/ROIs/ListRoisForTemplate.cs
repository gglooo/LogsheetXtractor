using FluentResults;
using ImTools;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.ROIs;

public sealed record ListRoisForTemplateQuery(Guid TemplateId);

public static class ListRoisForTemplateHandler
{
    public static Task<Result<IEnumerable<RoiDto>>> Handle(ListRoisForTemplateQuery request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var template = dbContext.Templates
            .AsNoTracking()
            .Include(t => t.Rois)
            .FindFirst(t => t.Id == request.TemplateId);

        if (template is null)
        {
            return Task.FromResult(Result.Fail<IEnumerable<RoiDto>>(new NotFoundError("Template not found")));
        }

        return Task.FromResult(Result.Ok(mapper.Map<IEnumerable<RoiDto>>(template.Rois)));
    }
}