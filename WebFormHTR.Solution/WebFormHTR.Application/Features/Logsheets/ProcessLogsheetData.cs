using System.ComponentModel.DataAnnotations;
using FluentResults;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record ProcessLogsheetDataCommand(Guid LogsheetId);

public static class ProcessLogsheetDataHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(ProcessLogsheetDataCommand request,
        IAppDbContext dbContext,
        ILogsheetService logsheetService,
        CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == request.LogsheetId, ct);
        if (logsheet is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Logsheet not found"));
        }

        try
        {
            var processResult = await logsheetService.ProcessLogsheetAsync(logsheet, ct);
            if (processResult.IsFailed)
            {
                return processResult;
            }

            await dbContext.SaveChangesAsync(ct);

            return processResult;
        }
        catch (Exception ex)
        {
            return Result.Fail<LogsheetDetailDto>($"Failed to process logsheet data: {ex.Message}");
        }
    }
}