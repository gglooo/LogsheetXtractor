using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Residuals;

public sealed record UpsertResidualCommand(Guid TemplateId, UpsertResidualDto Residual);

public static class UpsertResidualHandler
{
    public static async Task<Result<ResidualDto>> Handle(
        UpsertResidualCommand request,
        IResidualService residualService,
        IAppDbContext dbContext,
        CancellationToken ct
    )
    {
        var template = await dbContext.Templates.FindAsync(request.TemplateId, ct);
        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        try
        {
            var result = await residualService.UpsertResidualForTemplateAsync(
                request.TemplateId,
                request.Residual,
                ct
            );

            return result;
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to upsert residual: {ex.Message}");
        }
    }
}
