using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Logsheets;

public sealed record PatchLogsheetCommand(Guid Id, PatchLogsheetDto PatchLogsheet);

public static class PatchLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(
        PatchLogsheetCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var logsheet = await dbContext
            .Logsheets.Include(l => l.File)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }

        if (!logsheet.CanBeEdited())
        {
            return Result.Fail(
                new InvalidStateError(
                    "Logsheet properties cannot be patched while it is processing, needs review or is completed."
                )
            );
        }

        request.PatchLogsheet.Adapt(logsheet);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}
