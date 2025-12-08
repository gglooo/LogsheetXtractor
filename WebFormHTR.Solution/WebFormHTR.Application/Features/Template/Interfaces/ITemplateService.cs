using WebFormHTR.Application.Features.Template.DTOs;

namespace WebFormHTR.Application.Features.Template.Interfaces;

public interface ITemplateService
{
    Task<TemplateDetailDto> CloneTemplateAsync(Guid templateId, string newTemplateName, Guid fileId,
        CancellationToken cancellationToken);

    Task<TemplateDetailDto> CreateTemplateAsync(CreateTemplateCommand command, CancellationToken cancellationToken);
}