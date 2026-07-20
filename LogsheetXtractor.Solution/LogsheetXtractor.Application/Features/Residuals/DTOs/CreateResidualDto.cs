using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

/// <summary>
/// Input for creating a residual region on a template.
/// <param name="Content">The text expected in the residual region.</param>
/// <param name="Coordinates">The coordinates defining the residual region.</param>
/// </summary>
public record CreateResidualDto(string Content, Coordinates Coordinates);
