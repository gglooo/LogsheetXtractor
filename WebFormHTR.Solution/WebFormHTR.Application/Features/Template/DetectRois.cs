using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record DetectRoisCommand(Guid TemplateId);

public static class DetectRoisHandler
{
    public static async Task<Result<TemplateDetailDto>> Handle(DetectRoisCommand request, IRoiService roiService,
        IAppDbContext dbContext, IMapper mapper, CancellationToken ct)
    {
        var template = await dbContext.Templates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TemplateId, ct);

        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        try
        {
            var detectedRoid = await roiService.DetectRoisAsync(template.FileId, ct);
        }
        catch (Exception e)
        {
        }

        return await Task.FromResult(Result.Fail<TemplateDetailDto>("Not implemented"));
    }
}