using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record ProcessLogsheetDataCommand(Guid LogsheetId);

public static class ProcessLogsheetDataHandler
{
    public static async Task<Result> Handle(ProcessLogsheetDataCommand request, IAppDbContext dbContext)
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(ls => ls.Id == request.LogsheetId);
        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }
        
        throw new NotImplementedException();
    }
}