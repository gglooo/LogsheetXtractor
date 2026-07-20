using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

/// <summary>
/// Input for replacing the residual regions configured for a template.
/// <param name="Id">The existing residual identifier; null or empty identifies a new residual.</param>
/// <param name="Content">The text expected in the residual region.</param>
/// <param name="Coordinates">The coordinates defining the residual region.</param>
/// </summary>
public record SetResidualDto(Guid? Id, string Content, Coordinates Coordinates);
