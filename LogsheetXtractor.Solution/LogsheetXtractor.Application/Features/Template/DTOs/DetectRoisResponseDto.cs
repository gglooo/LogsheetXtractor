using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;

namespace LogsheetXtractor.Application.Features.Template.DTOs;

public record DetectRoisResponseDto(IEnumerable<RoiDto> Rois, IEnumerable<ResidualDto> Residuals);
