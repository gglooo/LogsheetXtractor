using FluentResults;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Logsheets;

public sealed record ListLogsheetsByTemplateQuery(Guid TemplateId);

public static class ListLogsheetsByTemplateHandler
{
    public static async Task<Result<IEnumerable<LogsheetListDto>>> Handle(
        ListLogsheetsByTemplateQuery request,
        IAppDbContext dbContext,
        IMapper mapper
    )
    {
        var logsheets = await dbContext
            .Logsheets.Include(l => l.File)
            .Where(ls => ls.TemplateId == request.TemplateId)
            .OrderByDescending(ls => ls.CreatedAt)
            .ToListAsync();

        return Result.Ok(mapper.Map<IEnumerable<LogsheetListDto>>(logsheets));
    }
}
