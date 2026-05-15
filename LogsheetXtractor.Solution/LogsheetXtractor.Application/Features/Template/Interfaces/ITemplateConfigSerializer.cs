using FluentResults;
using LogsheetXtractor.Domain.Entities;

namespace LogsheetXtractor.Application.Features.Template.Interfaces;

public interface ITemplateConfigSerializer
{
    Result<LogsheetXtractor.Domain.Entities.Template> DeserializeTemplateConfig(
        string importedConfig,
        Guid fileId,
        string templateName,
        Guid? parentId
    );

    Result<string> SerializeTemplateConfig(
        LogsheetXtractor.Domain.Entities.Template template,
        bool includeRoiValidations
    );
}
