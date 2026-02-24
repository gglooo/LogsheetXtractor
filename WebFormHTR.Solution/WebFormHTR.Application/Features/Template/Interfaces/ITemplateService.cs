using WebFormHTR.Application.Features.Template.DTOs;

namespace WebFormHTR.Application.Features.Template.Interfaces;

public interface ITemplateService
{
    Task<TemplateDetailDto> CloneTemplateAsync(Guid templateId, string newTemplateName, Guid fileId,
        CloneTemplateBacksideCommand? backside, CancellationToken cancellationToken);

    Task<TemplateDetailDto> CreateTemplateAsync(CreateTemplateCommand command, CancellationToken cancellationToken);
    Task<string> ExportTemplateConfigAsync(Guid templateId, CancellationToken cancellationToken);
}