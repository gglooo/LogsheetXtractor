using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public record AlignLogsheetCommand(
    Guid LogsheetId
);

public static class AlignLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(AlignLogsheetCommand request, IAppDbContext dbContext,
        IHtrScriptEngine scriptEngine, CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets.FindAsync(request.LogsheetId, ct);
        if (logsheet is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        try
        {
            var alignmentResult = await scriptEngine.AutomaticAlignAsync(new AutomaticAlignmentInputDto(logsheet), ct);

            await dbContext.SaveChangesAsync(ct);

            return Result.Ok(alignmentResult);
        }
        catch (Exception ex)
        {
            return Result.Fail<LogsheetDetailDto>($"Failed to align logsheet: {ex.Message}");
        }
    }
}