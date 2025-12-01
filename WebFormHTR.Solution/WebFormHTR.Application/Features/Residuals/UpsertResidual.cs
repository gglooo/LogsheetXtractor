using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Residuals;

public sealed record UpsertResidualCommand(Guid TemplateId, UpsertResidualDto Residual);

public static class UpsertResidualHandler
{
    public static async Task<Result<ResidualDto>> Handle(
        UpsertResidualCommand request,
        IResidualService residualService,
        IAppDbContext dbContext,
        CancellationToken ct)
    {
        var template = await dbContext.Templates.FindAsync(request.TemplateId, ct);
        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        try
        {
            var result = await residualService.UpsertResidualForTemplateAsync(request.TemplateId, request.Residual, ct);
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to upsert residual: {ex.Message}");
        }
    }
}
