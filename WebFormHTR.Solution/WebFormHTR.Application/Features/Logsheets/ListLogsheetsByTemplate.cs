using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record ListLogsheetsByTemplateQuery(Guid TemplateId);

public static class ListLogsheetsByTemplateHandler
{
    public static async Task<Result<IEnumerable<LogsheetListDto>>> Handle(
        ListLogsheetsByTemplateQuery request,
        IAppDbContext dbContext,
        IMapper mapper)
    {
        var logsheets = await dbContext.Logsheets
            .Where(ls => ls.TemplateId == request.TemplateId)
            .ToListAsync();

        return Result.Ok(mapper.Map<IEnumerable<LogsheetListDto>>(logsheets));
    }
}