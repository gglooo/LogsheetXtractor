using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services;

public class RoiService(IAppDbContext dbContext, IMapper mapper): IRoiService
{
    public async Task<IEnumerable<RoiDto>> SetRoisForTemplateAsync(Guid templateId, IEnumerable<SetRoiDto> updateRois, CancellationToken cancellationToken)
    {
        IList<Roi> allEntities = [];
        var updateRoisList = updateRois.ToList();
        
        var existingRois = await dbContext.Rois
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
}