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
    public static async Task<Result<IEnumerable<RoiDto>>> Handle(ListRoisForTemplateQuery request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var template = await dbContext.Templates
            .AsNoTracking()
            .Include(t => t.Rois)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, ct);

        if (template is null)
        {
            return Result.Fail<IEnumerable<RoiDto>>(new NotFoundError("Template not found"));
        }

        return Result.Ok(mapper.Map<IEnumerable<RoiDto>>(template.Rois));
    }
}