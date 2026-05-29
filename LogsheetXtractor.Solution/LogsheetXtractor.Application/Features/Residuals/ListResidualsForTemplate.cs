using FluentResults;
using ImTools;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Residuals;

public sealed record ListResidualsForTemplateQuery(Guid TemplateId);

public static class ListResidualsForTemplateHandler
{
    public static async Task<Result<IEnumerable<ResidualDto>>> Handle(
        ListResidualsForTemplateQuery request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct
    )
    {
        var template = await dbContext
            .Templates.AsNoTracking()
            .Include(t => t.Residuals)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, ct);

        if (template is null)
        {
            return Result.Fail<IEnumerable<ResidualDto>>(new NotFoundError("Template not found"));
        }

        return Result.Ok(mapper.Map<IEnumerable<ResidualDto>>(template.Residuals));
    }
}
