using FluentResults;
using LogsheetXtractor.Application.Features.Residuals.DTOs;

namespace LogsheetXtractor.Application.Features.Residuals;

public interface IResidualService
{
    Task<Result<IEnumerable<ResidualDto>>> SetResidualsForTemplateAsync(
        Guid templateId,
        IEnumerable<SetResidualDto> updateResiduals,
        CancellationToken cancellationToken
    );

    Task<Result<ResidualDto>> UpsertResidualForTemplateAsync(
        Guid templateId,
        UpsertResidualDto updateResidual,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<ResidualDto>>> UpsertResidualsForTemplateAsync(
        Guid templateId,
        IEnumerable<UpsertResidualDto> updateResiduals,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<ResidualDto>>> CloneResidualsForTemplateAsync(
        Guid sourceTemplateId,
        Guid targetTemplateId,
        CancellationToken cancellationToken
    );
}
