using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services;

public class ResidualService(IAppDbContext dbContext, IMapper mapper) : IResidualService
{
    public async Task<IEnumerable<ResidualDto>> SetResidualsForTemplateAsync(Guid templateId, IEnumerable<SetResidualDto> updateResiduals,
        CancellationToken cancellationToken)
    {
        IList<Residual> allEntities = [];
        var updateResidualsList = updateResiduals.ToList();

        var existingResiduals = await dbContext.Residuals
            .Include(r => r.Coordinates)
            .Where(r => r.TemplateId == templateId)
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        var updateIds = updateResidualsList
            .Where(x => x.Id != Guid.Empty && x.Id != null)
            .Select(x => x.Id)
            .ToHashSet();

        var residualsToDelete = existingResiduals
            .Values
            .Where(r => !updateIds.Contains(r.Id))
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

        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<IEnumerable<ResidualDto>>(allEntities);
    }

    public async Task<IEnumerable<ResidualDto>> UpsertResidualsForTemplateAsync(Guid templateId,
        IEnumerable<UpsertResidualDto> upsertResiduals,
        CancellationToken cancellationToken)
    {
        var template = await dbContext.Templates
            .Include(t => t.Residuals)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template is null)
        {
            throw new Exception("Template not found");
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

        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<IEnumerable<ResidualDto>>(allProcessedResiduals);
    }

    public Task<ResidualDto> UpsertResidualForTemplateAsync(Guid templateId, UpsertResidualDto updateResidual,
        CancellationToken cancellationToken)
    {
        return UpsertResidualsForTemplateAsync(templateId, [updateResidual], cancellationToken)
            .ContinueWith(t => t.Result.First(), cancellationToken);
    }
}
