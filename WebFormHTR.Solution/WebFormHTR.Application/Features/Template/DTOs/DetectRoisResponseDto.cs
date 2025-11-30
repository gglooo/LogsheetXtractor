using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;

namespace WebFormHTR.Application.Features.Template.DTOs;

public record DetectRoisResponseDto(
    IEnumerable<RoiDto> Rois,
    IEnumerable<ResidualDto> Residuals
);