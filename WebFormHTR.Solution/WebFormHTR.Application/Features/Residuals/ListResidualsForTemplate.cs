using FluentResults;
using ImTools;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Residuals;

public sealed record ListResidualsForTemplateQuery(Guid TemplateId);

public static class ListResidualsForTemplateHandler
{
    public static Task<Result<IEnumerable<ResidualDto>>> Handle(
        ListResidualsForTemplateQuery request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var template = dbContext.Templates
            .AsNoTracking()
            .Include(t => t.Residuals)
            .FindFirst(t => t.Id == request.TemplateId);

        if (template is null)
        {
            return Task.FromResult(Result.Fail<IEnumerable<ResidualDto>>(new NotFoundError("Template not found")));
        }

        return Task.FromResult(Result.Ok(mapper.Map<IEnumerable<ResidualDto>>(template.Residuals)));
    }
}
