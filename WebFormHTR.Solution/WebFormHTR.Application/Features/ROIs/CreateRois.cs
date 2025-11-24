using MapsterMapper;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.ROIs;

public sealed record CreateRoisCommand
(
    Guid TemplateId,
    IEnumerable<CreateRoiDto> Rois
);

public static class CreateRoisHandler
{
    public static async Task Handle(
        CreateRoisCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var roiEntities = mapper.Map<List<Roi>>(request.Rois);
        roiEntities.ForEach(r => r.TemplateId = request.TemplateId);

        await dbContext.Rois.AddRangeAsync(roiEntities, ct);
        await dbContext.SaveChangesAsync(ct);
    }
}