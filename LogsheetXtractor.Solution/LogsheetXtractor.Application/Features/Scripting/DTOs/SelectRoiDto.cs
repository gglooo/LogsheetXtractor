using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

/// <summary>
/// Input passed to the ROI-selection script for a template.
/// <param name="Template">The template whose PDF is analyzed for ROI and residual regions.</param>
/// </summary>
public record SelectRoisInputDto(LogsheetXtractor.Domain.Entities.Template Template);

/// <summary>
/// Parsed result returned by the ROI-selection script.
/// <param name="Rois">The regions identified as regions of interest.</param>
/// <param name="Residuals">The residual regions identified by the script, including their expected content.</param>
/// </summary>
public record SelectRoisOutputDto(IEnumerable<RoiDto> Rois, IEnumerable<ResidualDto> Residuals);
