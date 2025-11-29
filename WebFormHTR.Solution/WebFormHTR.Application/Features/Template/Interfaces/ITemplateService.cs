using WebFormHTR.Application.Features.Template.DTOs;

namespace WebFormHTR.Application.Features.Template.Interfaces;

public interface ITemplateService
{
    public Task<TemplateDetailDto> CloneTemplateAsync(Guid templateId, string newTemplateName, Guid fileId, CancellationToken cancellationToken);
}