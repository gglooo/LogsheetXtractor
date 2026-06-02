using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;

namespace LogsheetXtractor.Application.Features.Template.DTOs;

/// <summary>
/// TODO-DOC: Describe DetectRoisResponseDto purpose and usage.
/// <param name="Rois">TODO-DOC: Describe Rois.</param>
/// <param name="Residuals">TODO-DOC: Describe Residuals.</param>
/// </summary>
public record DetectRoisResponseDto(IEnumerable<RoiDto> Rois, IEnumerable<ResidualDto> Residuals);
