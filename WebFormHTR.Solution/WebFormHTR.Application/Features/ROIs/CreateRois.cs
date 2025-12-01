using FluentResults;
using MapsterMapper;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.ROIs;

public sealed record CreateRoisCommand(
    Guid TemplateId,
    IEnumerable<CreateRoiDto> Rois
);

public static class CreateRoisHandler
{
    public static async Task<Result<IEnumerable<RoiDto>>> Handle(
        CreateRoisCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var template = await dbContext.Templates.FindAsync(request.TemplateId, ct);
        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        var roiEntities = mapper.Map<List<Roi>>(request.Rois);
        roiEntities.ForEach(r => r.TemplateId = request.TemplateId);

        await dbContext.Rois.AddRangeAsync(roiEntities, ct);
        await dbContext.SaveChangesAsync(ct);

        return Result.Ok(mapper.Map<IEnumerable<RoiDto>>(roiEntities));
    }
}