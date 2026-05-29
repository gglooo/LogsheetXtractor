using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Residuals;

public class ResidualService(IAppDbContext dbContext, IMapper mapper) : IResidualService
{
    public async Task<Result<IEnumerable<ResidualDto>>> SetResidualsForTemplateAsync(
        Guid templateId,
        IEnumerable<SetResidualDto> updateResiduals,
        CancellationToken cancellationToken
    )
    {
        IList<Residual> allEntities = [];
        var updateResidualsList = updateResiduals.ToList();

        var existingResiduals = await dbContext
            .Residuals.Include(r => r.Coordinates)
            .Where(r => r.TemplateId == templateId)
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        var updateIds = updateResidualsList
            .Where(x => x.Id != Guid.Empty && x.Id != null)
            .Select(x => x.Id)
            .ToHashSet();

        var residualsToDelete = existingResiduals
            .Values.Where(r => !updateIds.Contains(r.Id))
            .ToList();

        if (residualsToDelete.Count != 0)
        {
            dbContext.Residuals.RemoveRange(residualsToDelete);
        }

        foreach (var dto in updateResidualsList)
        {
            if (dto.Id == Guid.Empty || dto.Id is null)
            {
                var residual = mapper.Map<Residual>(dto);

                residual.Id = Guid.NewGuid();
                residual.TemplateId = templateId;

                await dbContext.Residuals.AddAsync(residual, cancellationToken);

                allEntities.Add(residual);
            }
            else
            {
                var existingResidual = existingResiduals[dto.Id!.Value];
                mapper.Map(dto, existingResidual);

                allEntities.Add(existingResidual);
            }
        }

        return Result.Ok(mapper.Map<IEnumerable<ResidualDto>>(allEntities));
    }

    public async Task<Result<IEnumerable<ResidualDto>>> UpsertResidualsForTemplateAsync(
        Guid templateId,
        IEnumerable<UpsertResidualDto> upsertResiduals,
        CancellationToken cancellationToken
    )
    {
        var template = await dbContext
            .Templates.Include(t => t.Residuals)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
        }

        var existingResidualsMap = template.Residuals.ToDictionary(r => r.Id);

        var newResiduals = new List<Residual>();
        var allProcessedResiduals = new List<Residual>();

        foreach (var dto in upsertResiduals)
        {
            if (existingResidualsMap.TryGetValue(dto.Id ?? Guid.Empty, out var existingResidual))
            {
                mapper.Map(dto, existingResidual);
                allProcessedResiduals.Add(existingResidual);
            }
            else
            {
                var newResidual = mapper.Map<Residual>(dto);
                newResidual.TemplateId = templateId;

                newResiduals.Add(newResidual);
                allProcessedResiduals.Add(newResidual);
            }
        }

        if (newResiduals.Any())
        {
            await dbContext.Residuals.AddRangeAsync(newResiduals, cancellationToken);
        }

        return Result.Ok(mapper.Map<IEnumerable<ResidualDto>>(allProcessedResiduals));
    }

    public async Task<Result<ResidualDto>> UpsertResidualForTemplateAsync(
        Guid templateId,
        UpsertResidualDto updateResidual,
        CancellationToken cancellationToken
    )
    {
        var result = await UpsertResidualsForTemplateAsync(
            templateId,
            [updateResidual],
            cancellationToken
        );
        if (result.IsFailed)
        {
            return result.ToResult();
        }

        return Result.Ok(result.Value.First());
    }

    public async Task<Result<IEnumerable<ResidualDto>>> CloneResidualsForTemplateAsync(
        Guid sourceTemplateId,
        Guid targetTemplateId,
        CancellationToken cancellationToken
    )
    {
        var sourceResiduals = await dbContext
            .Residuals.AsNoTracking()
            .Where(r => r.TemplateId == sourceTemplateId)
            .ToListAsync(cancellationToken);

        var clonedResiduals = sourceResiduals
            .Select(r =>
            {
                var cloned = mapper.Map<Residual>(r);
                cloned.Id = Guid.NewGuid();
                cloned.TemplateId = targetTemplateId;
                return cloned;
            })
            .ToList();

        await dbContext.Residuals.AddRangeAsync(clonedResiduals, cancellationToken);

        return Result.Ok(mapper.Map<IEnumerable<ResidualDto>>(clonedResiduals));
    }
}
