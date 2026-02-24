using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template.CreateTemplate;

public sealed record CreateTemplateCommand
{
    public string Name { get; set; } = string.Empty;
    public Guid FileId { get; set; }
    public Guid? ParentId { get; set; }
    public string? ImportedConfig { get; set; }
    public CreateTemplateBacksideCommand? Backside { get; set; }
}

public sealed record CreateTemplateBacksideCommand
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid FileId { get; set; }
    public string? ImportedConfig { get; set; }
}

public static class CreateTemplateHandler
{
    public static async Task<Result<TemplateDetailDto>> Handle(CreateTemplateCommand request, IAppDbContext dbContext,
        ITemplateService templateService, CancellationToken ct)
    {
        if (request.ParentId is not null &&
            !await dbContext.Templates
                .AnyAsync(t => t.Id == request.ParentId.Value, ct))
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("Parent template not found"));
        }

        if (request.Backside?.ParentId is not null &&
            !await dbContext.Templates
                .AnyAsync(t => t.Id == request.Backside.ParentId.Value, ct))
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("Backside parent template not found"));
        }

        if (!await dbContext.Files
                .AnyAsync(f => f.Id == request.FileId, ct))
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("File not found"));
        }

        if (request.Backside is not null &&
            !await dbContext.Files.AnyAsync(f => f.Id == request.Backside.FileId, ct))
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("Backside file not found"));
        }

        try
        {
            return await templateService.CreateTemplateAsync(request, ct);
        }
        catch (Exception e)
        {
            return Result.Fail<TemplateDetailDto>(e.Message);
        }
    }
}