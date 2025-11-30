using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;

namespace WebFormHTR.Application.Features.ROIs;

public interface IRoiService
{
    Task<IEnumerable<RoiDto>> SetRoisForTemplateAsync(Guid templateId, IEnumerable<SetRoiDto> updateRois,
        CancellationToken cancellationToken);

    Task<RoiDto> UpsertRoiForTemplateAsync(Guid templateId, UpsertRoiDto updateRoi,
        CancellationToken cancellationToken);

    Task<IEnumerable<RoiDto>> UpsertRoisForTemplateAsync(Guid templateId, IEnumerable<UpsertRoiDto> updateRois,
        CancellationToken cancellationToken);

    Task<DetectRoisResponseDto> DetectRoisAsync(Guid fileId, Guid templateId, CancellationToken cancellationToken);
}