using FluentResults;
using ImTools;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services;

public class RoiService(IAppDbContext dbContext, IMapper mapper, IHtrScriptEngine scriptEngine) : IRoiService
{
    public async Task<IEnumerable<RoiDto>> SetRoisForTemplateAsync(
        Guid templateId,
        IEnumerable<SetRoiDto> updateRois,
        CancellationToken cancellationToken)
    {
        var updateRoisList = updateRois.ToList();

        var existingRois = await dbContext.Rois
            .Include(r => r.Coordinates)
            .Where(r => r.TemplateId == templateId)
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        var incomingGuids = updateRoisList
            .Select(x => Guid.TryParse(x.Id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToHashSet();

        var roisToDelete = existingRois.Values
            .Where(r => !incomingGuids.Contains(r.Id))
            .ToList();

        if (roisToDelete.Any())
        {
            dbContext.Rois.RemoveRange(roisToDelete);
        }

        var processedEntities = new List<Roi>();
        foreach (var dto in updateRoisList)
        {
            Roi entity;
            var isValidGuid = Guid.TryParse(dto.Id, out var guid);

            if (isValidGuid && existingRois.TryGetValue(guid, out var existingRoi))
            {
                mapper.Map(dto, existingRoi);
                entity = existingRoi;
            }
            else
            {
                entity = mapper.Map<Roi>(dto);
                entity.Id = Guid.NewGuid();
                entity.TemplateId = templateId;

                await dbContext.Rois.AddAsync(entity, cancellationToken);
            }

            processedEntities.Add(entity);
        }

        return mapper.Map<IEnumerable<RoiDto>>(processedEntities);
    }

    public async Task<IEnumerable<RoiDto>> UpsertRoisForTemplateAsync(Guid templateId,
        IEnumerable<UpsertRoiDto> upsertRois,
        CancellationToken cancellationToken)
    {
        var template = await dbContext.Templates
            .Include(t => t.Rois)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template is null)
        {
            throw new Exception("Template not found");
        }

        var existingRoisMap = template.Rois.ToDictionary(r => r.Id);

        var newRois = new List<Roi>();
        var allProcessedRois = new List<Roi>();

        foreach (var dto in upsertRois)
        {
            if (existingRoisMap.TryGetValue(dto.Id ?? Guid.Empty, out var existingRoi))
            {
                mapper.Map(dto, existingRoi);
                allProcessedRois.Add(existingRoi);
            }
            else
            {
                var newRoi = mapper.Map<Roi>(dto);
                newRoi.TemplateId = templateId;

                newRois.Add(newRoi);
                allProcessedRois.Add(newRoi);
            }
        }

        if (newRois.Any())
        {
            await dbContext.Rois.AddRangeAsync(newRois, cancellationToken);
        }

        return mapper.Map<IEnumerable<RoiDto>>(allProcessedRois);
    }

    public Task<RoiDto> UpsertRoiForTemplateAsync(Guid templateId, UpsertRoiDto updateRoi,
        CancellationToken cancellationToken)
    {
        return UpsertRoisForTemplateAsync(templateId, [updateRoi], cancellationToken)
            .ContinueWith(t => t.Result.First(), cancellationToken);
    }

    public async Task<DetectRoisResponseDto> DetectRoisAsync(
        Template template,
        CancellationToken cancellationToken)
    {
        var input = new SelectRoisInputDto(template);

        var result = await scriptEngine.SelectRoisAsync(input, cancellationToken);

        return mapper.Map<DetectRoisResponseDto>(result);
    }

    public async Task<IEnumerable<RoiDto>> CloneRoisForTemplateAsync(Guid sourceTemplateId, Guid targetTemplateId,
        CancellationToken cancellationToken)
    {
        var sourceRois = await dbContext.Rois
            .AsNoTracking()
            .Where(r => r.TemplateId == sourceTemplateId)
            .ToListAsync(cancellationToken);

        var clonedRois = sourceRois.Select(r =>
        {
            var clonedRoi = mapper.Map<Roi>(r);
            clonedRoi.Id = Guid.NewGuid();
            clonedRoi.TemplateId = targetTemplateId;
            return clonedRoi;
        }).ToList();

        await dbContext.Rois.AddRangeAsync(clonedRois, cancellationToken);

        return mapper.Map<IEnumerable<RoiDto>>(clonedRois);
    }
}