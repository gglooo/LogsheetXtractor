using FluentResults;
using MapsterMapper;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.Residuals;

public sealed record CreateResidualCommand(
    Guid TemplateId,
    IEnumerable<CreateResidualDto> Residuals
);

public static class CreateResidualHandler
{
    public static async Task<Result<IEnumerable<ResidualDto>>> Handle(
        CreateResidualCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var template = await dbContext.Templates.FindAsync(request.TemplateId, ct);
        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        var residualEntities = mapper.Map<List<Residual>>(request.Residuals);
        residualEntities.ForEach(r => r.TemplateId = request.TemplateId);

        await dbContext.Residuals.AddRangeAsync(residualEntities, ct);
        await dbContext.SaveChangesAsync(ct);

        return Result.Ok(mapper.Map<IEnumerable<ResidualDto>>(residualEntities));
    }
}