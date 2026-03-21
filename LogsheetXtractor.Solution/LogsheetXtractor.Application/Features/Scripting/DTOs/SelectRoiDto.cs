using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

public record SelectRoisInputDto(LogsheetXtractor.Domain.Entities.Template Template);

public record SelectRoisOutputDto(IEnumerable<RoiDto> Rois, IEnumerable<ResidualDto> Residuals);
