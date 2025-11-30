using WebFormHTR.Application.Features.ROIs.DTOs;

namespace WebFormHTR.Application.Features.Scripting.DTOs;

public record SelectRoisInputDto(string FilePath, Guid TemplateId);

// TODO: add list of residuals
public record SelectRoisOutputDto(List<RoiDto> Rois);