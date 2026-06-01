using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Logsheets;

public sealed record BatchDeleteLogsheetCommand(Guid[] LogsheetIds);

public static class DeleteBatchLogsheetHandler
{
    public static async Task<Result> Handle(
        BatchDeleteLogsheetCommand request,
        IFileService fileService,
        CancellationToken ct,
        IAppDbContext dbContext
    )
    {
        var existingLogsheets = await dbContext
            .Logsheets.Where(l => request.LogsheetIds.AsEnumerable().Contains(l.Id))
            .ToListAsync(ct);

        if (existingLogsheets.Count != request.LogsheetIds.Length)
        {
            return Result.Fail(new NotFoundError("One or more logsheets not found"));
        }

        dbContext.Logsheets.RemoveRange(existingLogsheets);
        await fileService.DeleteFilesAsync(existingLogsheets.Select(l => l.FileId));

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
