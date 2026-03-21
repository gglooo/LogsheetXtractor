using FluentResults;
using LogsheetXtractor.Application.Features.Template.CreateTemplate;
using LogsheetXtractor.Application.Features.Template.DTOs;

namespace LogsheetXtractor.Application.Features.Template.Interfaces;

public interface ITemplateService
{
    Task<Result<TemplateDetailDto>> AddBacksideTemplateAsync(
        Guid templateId,
        string name,
        Guid fileId,
        CancellationToken cancellationToken
    );

    Task<Result<TemplateDetailDto>> CloneTemplateAsync(
        Guid templateId,
        string newTemplateName,
        Guid fileId,
        CloneTemplateBacksideCommand? backside,
        CancellationToken cancellationToken
    );

    Task<Result<TemplateDetailDto>> CreateTemplateAsync(
        CreateTemplateCommand command,
        CancellationToken cancellationToken
    );

    Task<Result<string>> ExportTemplateConfigAsync(
        Guid templateId,
        CancellationToken cancellationToken
    );
}
