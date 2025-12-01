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
    public async Task<IEnumerable<RoiDto>> SetRoisForTemplateAsync(Guid templateId, IEnumerable<SetRoiDto> updateRois,
        CancellationToken cancellationToken)
    {
        IList<Roi> allEntities = [];
        var updateRoisList = updateRois.ToList();

        var existingRois = await dbContext.Rois
            .Include(r => r.Coordinates)
            .Where(r => r.TemplateId == templateId)
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        var updateIds = updateRoisList
            .Where(x => x.Id != Guid.Empty && x.Id != null)
            .Select(x => x.Id)
            .ToHashSet();

        var roisToDelete = existingRois
            .Values
            .Where(r => !updateIds.Contains(r.Id))
            .ToList();

        if (roisToDelete.Count != 0)
        {
            dbContext.Rois.RemoveRange(roisToDelete);
        }

        foreach (var dto in updateRoisList)
        {
            if (dto.Id == Guid.Empty || dto.Id is null)
            {
                var roi = mapper.Map<Roi>(dto);

                roi.Id = Guid.NewGuid();
                roi.TemplateId = templateId;

                await dbContext.Rois.AddAsync(roi, cancellationToken);

                allEntities.Add(roi);
            }
            else
            {
                var existingRoi = existingRois[dto.Id!.Value];
                mapper.Map(dto, existingRoi);

                allEntities.Add(existingRoi);
            }
        }

        return mapper.Map<IEnumerable<RoiDto>>(allEntities);
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

    public async Task<DetectRoisResponseDto> DetectRoisAsync(Guid fileId,
        Guid templateId,
        CancellationToken cancellationToken)
    {
        var file = await dbContext.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);

        if (file is null)
        {
            throw new Exception("Template not found");
        }

        var input = new SelectRoisInputDto(file.StoragePath, templateId);

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