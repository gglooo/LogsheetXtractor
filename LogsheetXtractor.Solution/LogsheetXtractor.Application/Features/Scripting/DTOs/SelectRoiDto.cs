using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

/// <summary>
/// TODO-DOC: Describe SelectRoisInputDto purpose and usage.
/// <param name="Residuals">TODO-DOC: Describe Residuals.</param>
/// </summary>
public record SelectRoisInputDto(LogsheetXtractor.Domain.Entities.Template Template);

/// <summary>
/// TODO-DOC: Describe SelectRoisOutputDto purpose and usage.
/// <param name="Rois">TODO-DOC: Describe Rois.</param>
/// <param name="Residuals">TODO-DOC: Describe Residuals.</param>
/// </summary>
public record SelectRoisOutputDto(IEnumerable<RoiDto> Rois, IEnumerable<ResidualDto> Residuals);
