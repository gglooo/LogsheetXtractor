using FluentResults;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record PatchLogsheetCommand
(
    Guid Id,
    PatchLogsheetDto PatchLogsheet
);

public static class PatchLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(PatchLogsheetCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var logsheet = await dbContext.Logsheets
            .Include(l => l.File)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        request.PatchLogsheet.Adapt(logsheet);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}