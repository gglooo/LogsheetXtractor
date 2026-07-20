using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;

namespace LogsheetXtractor.Application.Features.Template.DTOs;

/// <summary>
/// Result of detecting ROI and residual regions in a template.
/// <param name="Rois">The detected regions of interest.</param>
/// <param name="Residuals">The detected residual regions and their expected content.</param>
/// </summary>
public record DetectRoisResponseDto(IEnumerable<RoiDto> Rois, IEnumerable<ResidualDto> Residuals);
