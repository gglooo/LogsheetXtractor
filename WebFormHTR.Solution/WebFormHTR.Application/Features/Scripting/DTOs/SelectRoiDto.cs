using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;

namespace WebFormHTR.Application.Features.Scripting.DTOs;

public record SelectRoisInputDto(Domain.Entities.Template Template);

public record SelectRoisOutputDto(IEnumerable<RoiDto> Rois, IEnumerable<ResidualDto> Residuals);