using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

/// <summary>
/// TODO-DOC: Describe UpsertResidualDto purpose and usage.
/// <param name="Id">TODO-DOC: Describe Id.</param>
/// <param name="Content">TODO-DOC: Describe Content.</param>
/// <param name="Coordinates">TODO-DOC: Describe Coordinates.</param>
/// </summary>
public record UpsertResidualDto(Guid? Id, string Content, Coordinates Coordinates);
