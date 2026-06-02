using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

/// <summary>
/// TODO-DOC: Describe CreateResidualDto purpose and usage.
/// <param name="Content">TODO-DOC: Describe Content.</param>
/// <param name="Coordinates">TODO-DOC: Describe Coordinates.</param>
/// </summary>
public record CreateResidualDto(string Content, Coordinates Coordinates);
