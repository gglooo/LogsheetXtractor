using WebFormHTR.Application.Features.ROIs.DTOs;

namespace WebFormHTR.Application.Features.ROIs;

public interface IRoiService
{
    public Task<IEnumerable<RoiDto>> SetRoisForTemplateAsync(Guid templateId, IEnumerable<SetRoiDto> updateRois, CancellationToken cancellationToken);
    public Task<RoiDto> UpsertRoiForTemplateAsync(Guid templateId, UpsertRoiDto updateRoi, CancellationToken cancellationToken);
}