using FluentResults;
using ImTools;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services;

public class RoiService(IAppDbContext dbContext, IMapper mapper) : IRoiService
{
    public async Task<IEnumerable<RoiDto>> SetRoisForTemplateAsync(Guid templateId, IEnumerable<SetRoiDto> updateRois, CancellationToken cancellationToken)
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

    public async Task<RoiDto> UpsertRoiForTemplateAsync(Guid templateId, UpsertRoiDto updateRoi, CancellationToken cancellationToken)
    {
        var template = dbContext.Templates.FindFirst(t => t.Id == templateId);
        if (template is null)
        {
            throw new Exception("Template not found");
        }

        var existingRoi = template.Rois.FirstOrDefault(r => r.Id == updateRoi.Id);
        if (existingRoi is not null)
        {
            mapper.Map(updateRoi, existingRoi);
            return mapper.Map<RoiDto>(existingRoi);
        }

        var roi = mapper.Map<Roi>(updateRoi);
        roi.TemplateId = templateId;

        await dbContext.Rois.AddAsync(roi, cancellationToken);
        return mapper.Map<RoiDto>(roi);
    }
}