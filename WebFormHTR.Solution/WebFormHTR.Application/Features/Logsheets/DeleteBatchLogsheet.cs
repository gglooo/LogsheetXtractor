using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record BatchDeleteLogsheetCommand(
    Guid[] LogsheetIds
);

public static class DeleteBatchLogsheetHandler
{
    public static async Task<Result> Handle(
        BatchDeleteLogsheetCommand request,
        IFileService fileService,
        CancellationToken ct,
        IAppDbContext dbContext)
    {
        var existingLogsheets = await dbContext.Logsheets
            .Where(l => request.LogsheetIds.Contains(l.Id))
            .ToListAsync(ct);

        if (existingLogsheets.Count != request.LogsheetIds.Length)
        {
            return Result.Fail(new NotFoundError("One or more logsheets not found"));
        }

        dbContext.Logsheets.RemoveRange(existingLogsheets);
        await fileService.DeleteFilesAsync(request.LogsheetIds);

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}