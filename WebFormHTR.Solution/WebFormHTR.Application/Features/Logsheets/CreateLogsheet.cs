using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record CreateLogsheetCommand
(
    Guid TemplateId,
    Guid FileId
);

public static class CreateLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(CreateLogsheetCommand request, CancellationToken ct, IAppDbContext dbContext, IMapper mapper)
    {
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == request.FileId, cancellationToken: ct);
        if (file is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("File not found"));
        }
        
        var fileIsAssignedToAnotherLogsheet = await dbContext.Logsheets.AnyAsync(l => l.FileId == request.FileId, cancellationToken: ct);
        if (fileIsAssignedToAnotherLogsheet)
        {
            return Result.Fail<LogsheetDetailDto>(new ConstraintError("File is already assigned to another logsheet"));
        }
        
        var template = await dbContext.Templates.FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken: ct);
        if (template is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Template not found"));
        }
        
        var logsheet = mapper.Map<Domain.Entities.Logsheet>(request);
        
        await dbContext.Logsheets.AddAsync(logsheet, ct);
        await dbContext.SaveChangesAsync(ct);
        
        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}