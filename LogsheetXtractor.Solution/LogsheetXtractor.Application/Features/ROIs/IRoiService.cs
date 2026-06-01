using FluentResults;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Application.Features.Template.DTOs;

namespace LogsheetXtractor.Application.Features.ROIs;

public interface IRoiService
{
    Task<Result<IEnumerable<RoiDto>>> SetRoisForTemplateAsync(
        Guid templateId,
        IEnumerable<SetRoiDto> updateRois,
        CancellationToken cancellationToken
    );

    Task<Result<DetectRoisResponseDto>> DetectRoisAsync(
        LogsheetXtractor.Domain.Entities.Template template,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<RoiDto>>> CloneRoisForTemplateAsync(
        Guid sourceTemplateId,
        Guid targetTemplateId,
        CancellationToken cancellationToken
    );
}
