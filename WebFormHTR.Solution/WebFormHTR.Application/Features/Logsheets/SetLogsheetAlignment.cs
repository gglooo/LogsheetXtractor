using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record SetLogsheetAlignmentCommand(Guid LogsheetId, AlignmentDataDto AlignmentData);

public static class SetLogsheetAlignmentHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(SetLogsheetAlignmentCommand command,
        IAppDbContext dbContext, IMapper mapper, CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == command.LogsheetId, ct);
        if (logsheet is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        logsheet.AlignmentDataModelConfig = mapper.Map<AlignmentContainer>(command.AlignmentData);

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}