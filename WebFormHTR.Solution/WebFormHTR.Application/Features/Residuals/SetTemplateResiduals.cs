using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Residuals;

public sealed record SetTemplateResidualsCommand(Guid TemplateId, IEnumerable<SetResidualDto> Residuals);

public static class SetTemplateResidualsHandler
{
    public static async Task<Result<IEnumerable<ResidualDto>>> Handle(
        SetTemplateResidualsCommand request,
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
            var result = await residualService.SetResidualsForTemplateAsync(request.TemplateId, request.Residuals, ct);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to set residuals: {ex.Message}");
        }
    }
}