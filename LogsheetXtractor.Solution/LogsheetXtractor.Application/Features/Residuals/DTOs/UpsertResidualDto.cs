using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

/// <summary>
/// Input for creating or updating one residual region on a template.
/// <param name="Id">The residual identifier to update; null or empty causes a new residual to be created.</param>
/// <param name="Content">The text expected in the residual region.</param>
/// <param name="Coordinates">The coordinates defining the residual region.</param>
/// </summary>
public record UpsertResidualDto(Guid? Id, string Content, Coordinates Coordinates);
