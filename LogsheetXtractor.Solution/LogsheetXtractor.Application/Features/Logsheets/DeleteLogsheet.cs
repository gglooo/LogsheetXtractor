using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Logsheets;

public sealed record DeleteLogsheetCommand(Guid Id);

public static class DeleteLogsheetHandler
{
    public static async Task<Result> Handle(
        DeleteLogsheetCommand request,
        IFileService fileService,
        IAppDbContext dbContext,
        CancellationToken ct
    )
    {
        var logsheet = dbContext.Logsheets.FirstOrDefault(l => l.Id == request.Id);
        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }

        dbContext.Logsheets.Remove(logsheet);
        await fileService.DeleteFileAsync(logsheet.FileId);

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
