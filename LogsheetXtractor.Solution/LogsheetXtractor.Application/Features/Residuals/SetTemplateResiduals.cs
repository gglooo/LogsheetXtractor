using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Residuals;

public sealed record SetTemplateResidualsCommand(
    Guid TemplateId,
    IEnumerable<SetResidualDto> Residuals
);

public static class SetTemplateResidualsHandler
{
    public static async Task<Result<IEnumerable<ResidualDto>>> Handle(
        SetTemplateResidualsCommand request,
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
            var result = await residualService.SetResidualsForTemplateAsync(
                request.TemplateId,
                request.Residuals,
                ct
            );

            await dbContext.SaveChangesAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to set residuals: {ex.Message}");
        }
    }
}
