using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.Create.Events;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Interfaces;
using Wolverine;

namespace WebFormHTR.Application.Features.Logsheets.Create;

public sealed record CreateLogsheetCommand(
    Guid TemplateId,
    Guid? BacksideTemplateId,
    Guid FileId,
    bool PerformAutomaticAlignment = true
);

public static class CreateLogsheetHandler
{
    public static async Task<Result<LogsheetDetailDto>> Handle(CreateLogsheetCommand request, CancellationToken ct,
        IAppDbContext dbContext,
        IMapper mapper,
        IMessageBus bus)
    {
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == request.FileId, ct);
        if (file is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("File not found"));
        }

        var fileIsAssignedToAnotherLogsheet = await dbContext.Logsheets.AnyAsync(l => l.FileId == request.FileId, ct);
        if (fileIsAssignedToAnotherLogsheet)
        {
            return Result.Fail<LogsheetDetailDto>(new ConstraintError("File is already assigned to another logsheet"));
        }

        var templates = await dbContext.Templates
            .Where(t => t.Id == request.TemplateId || t.Id == request.BacksideTemplateId)
            .ToListAsync(ct);
        if (templates.Count != (request.BacksideTemplateId.HasValue ? 2 : 1))
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Template not found"));
        }

        var logsheet = mapper.Map<Domain.Entities.Logsheet>(request);

        await dbContext.Logsheets.AddAsync(logsheet, ct);
        await bus.PublishAsync(new LogsheetCreatedEvent(logsheet.Id, request.PerformAutomaticAlignment));

        await dbContext.SaveChangesAsync(ct);


        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}