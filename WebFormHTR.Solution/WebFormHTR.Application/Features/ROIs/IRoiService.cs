using FluentResults;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;

namespace WebFormHTR.Application.Features.ROIs;

public interface IRoiService
{
    Task<Result<IEnumerable<RoiDto>>> SetRoisForTemplateAsync(Guid templateId, IEnumerable<SetRoiDto> updateRois,
        CancellationToken cancellationToken);

    Task<Result<DetectRoisResponseDto>> DetectRoisAsync(Domain.Entities.Template template, CancellationToken cancellationToken);

    Task<Result<IEnumerable<RoiDto>>> CloneRoisForTemplateAsync(Guid sourceTemplateId, Guid targetTemplateId,
        CancellationToken cancellationToken);
}
