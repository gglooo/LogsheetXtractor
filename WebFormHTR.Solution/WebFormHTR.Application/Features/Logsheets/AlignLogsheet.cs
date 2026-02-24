using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace WebFormHTR.Application.Features.Logsheets;

public record AlignLogsheetCommand(
    Guid LogsheetId
);

public static class AlignLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(AlignLogsheetCommand request, IAppDbContext dbContext,
        IHtrScriptEngine scriptEngine, ILogger<AlignLogsheetCommand> logger, CancellationToken ct)
    {
        logger.LogInformation("Starting automatic alignment for Logsheet {LogsheetId}", request.LogsheetId);

        var logsheet = await dbContext.Logsheets.FindAsync(request.LogsheetId, ct);
        if (logsheet is null)
        {
            logger.LogWarning("Logsheet {LogsheetId} not found for alignment", request.LogsheetId);
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        try
        {
            var alignmentResult = await scriptEngine.AutomaticAlignAsync(new AutomaticAlignmentInputDto(logsheet), ct);
            if (alignmentResult.IsFailed)
            {
                return alignmentResult.ToResult();
            }

            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation("Automatic alignment completed successfully for Logsheet {LogsheetId}",
                request.LogsheetId);

            return alignmentResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Automatic alignment failed for Logsheet {LogsheetId}", request.LogsheetId);
            return Result.Fail<LogsheetDetailDto>($"Failed to align logsheet: {ex.Message}");
        }
    }
}