using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Logsheets.Create.Events;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace LogsheetXtractor.Application.Features.Logsheets.Create;

/// <summary>
/// Command for creating a logsheet bound to template and file inputs.
/// </summary>
/// <param name="TemplateId">Identifier of the frontside template.</param>
/// <param name="BacksideTemplateId">Optional identifier of the backside template.</param>
/// <param name="FileId">Identifier of the source file.</param>
/// <param name="PerformAutomaticAlignment">Whether automatic alignment should be triggered after creation.</param>
public sealed record CreateLogsheetCommand(
    Guid TemplateId,
    Guid? BacksideTemplateId,
    Guid FileId,
    bool PerformAutomaticAlignment = true
);

/// <summary>
/// Handles creation of new logsheets and emits creation events.
/// </summary>
public static class CreateLogsheetHandler
{
    /// <summary>
    /// Validates command references, creates a logsheet, and publishes a creation event.
    /// </summary>
    public static async Task<Result<LogsheetDetailDto>> Handle(
        CreateLogsheetCommand request,
        CancellationToken ct,
        IAppDbContext dbContext,
        IMapper mapper,
        IMessageBus bus
    )
    {
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == request.FileId, ct);
        if (file is null)
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("File not found"));
        }

        var fileIsAssignedToAnotherLogsheet = await dbContext.Logsheets.AnyAsync(
            l => l.FileId == request.FileId,
            ct
        );
        if (fileIsAssignedToAnotherLogsheet)
        {
            return Result.Fail<LogsheetDetailDto>(
                new ConstraintError("File is already assigned to another logsheet")
            );
        }

        var templates = await dbContext
            .Templates.Where(t => t.Id == request.TemplateId || t.Id == request.BacksideTemplateId)
            .ToListAsync(ct);
        if (templates.Count != (request.BacksideTemplateId.HasValue ? 2 : 1))
        {
            return Result.Fail<LogsheetDetailDto>(new NotFoundError("Template not found"));
        }

        var logsheet = mapper.Map<Domain.Entities.Logsheet>(request);

        await dbContext.Logsheets.AddAsync(logsheet, ct);
        await bus.PublishAsync(
            new LogsheetCreatedEvent(logsheet.Id, request.PerformAutomaticAlignment)
        );

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok(mapper.Map<LogsheetDetailDto>(logsheet));
    }
}
