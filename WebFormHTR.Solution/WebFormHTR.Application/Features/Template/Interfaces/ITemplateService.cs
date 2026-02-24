using FluentResults;
using WebFormHTR.Application.Features.Template.DTOs;

namespace WebFormHTR.Application.Features.Template.Interfaces;

public interface ITemplateService
{
    Task<Result<TemplateDetailDto>> CloneTemplateAsync(Guid templateId, string newTemplateName, Guid fileId,
        CloneTemplateBacksideCommand? backside, CancellationToken cancellationToken);

    Task<Result<TemplateDetailDto>> CreateTemplateAsync(CreateTemplateCommand command, CancellationToken cancellationToken);
    Task<Result<string>> ExportTemplateConfigAsync(Guid templateId, CancellationToken cancellationToken);
}