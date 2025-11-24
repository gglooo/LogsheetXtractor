using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record DeleteLogsheetCommand(Guid Id);

public static class DeleteLogsheetHandler
{
    public static async Task<Result> Handle(DeleteLogsheetCommand request, IAppDbContext dbContext, CancellationToken ct)
    {
        var logsheet = dbContext.Logsheets.FirstOrDefault(l => l.Id == request.Id);
        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }
        
        dbContext.Logsheets.Remove(logsheet);
        await dbContext.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}