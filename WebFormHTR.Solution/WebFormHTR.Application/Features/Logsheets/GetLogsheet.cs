using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record GetLogsheetQuery(
    Guid Id
);

public static class GetLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(GetLogsheetQuery request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var logsheet = await dbContext.Logsheets
            .Include(l => l.File)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }

        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}