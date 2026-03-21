using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Template.Interfaces;

namespace LogsheetXtractor.Application.Features.Template;

public sealed record ExportTemplateConfigQuery(Guid Id);

public static class ExportTemplateHandler
{
    public static async Task<Result<GetFileDto>> Handle(
        ExportTemplateConfigQuery request,
        ITemplateService templateService,
        IFileService fileService,
        CancellationToken ct
    )
    {
        try
        {
            var configResult = await templateService.ExportTemplateConfigAsync(request.Id, ct);
            if (configResult.IsFailed)
            {
                return configResult.ToResult();
            }

            var fileDto = await fileService.GetFileFromContentAsync(
                System.Text.Encoding.UTF8.GetBytes(configResult.Value),
                $"template_{request.Id}_config.json",
                "application/json",
                ct
            );

            if (fileDto is null)
            {
                return Result.Fail<GetFileDto>("Failed to create file from template config");
            }

            return fileDto;
        }
        catch (Exception e)
        {
            return Result.Fail<GetFileDto>(e.Message);
        }
    }
}
