using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record CreateTemplateCommand
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid? FileId { get; set; }
}

public static class CreateTemplateHandler
{
    public static async Task<Result<TemplateDetailDto>> Handle(CreateTemplateCommand request, IAppDbContext dbContext, IMapper mapper, CancellationToken ct)
    {

        if (request.ParentId is not null && 
            !await dbContext.Templates
                .AnyAsync(t => t.Id == request.ParentId.Value, ct) )
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("Parent template not found"));
        }
        
        if (request.FileId is not null && !await dbContext.Files
                .AnyAsync(f => f.Id == request.FileId.Value, ct))
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("File not found"));
        }
        
        var template = new Domain.Entities.Template
        {
            Name = request.Name,
            ParentId = request.ParentId,
            FileId = request.FileId
        };

        dbContext.Templates.Add(template);
        await dbContext.SaveChangesAsync(ct);

        return Result.Ok(mapper.Map<TemplateDetailDto>(template));
    }
}