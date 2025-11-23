using FluentResults;
using MapsterMapper;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record GetTemplateQuery(string Id);

public static class GetTemplateHandler
{
    public static Task<Result<TemplateDetailDto?>> Handle(GetTemplateQuery request, IAppDbContext dbContext, IMapper mapper)
    {
        var template = dbContext.Templates.FirstOrDefault(t => t.Id.ToString() == request.Id);
        return Task.FromResult(Result.Ok(mapper.Map<TemplateDetailDto?>(template)));
    }
}