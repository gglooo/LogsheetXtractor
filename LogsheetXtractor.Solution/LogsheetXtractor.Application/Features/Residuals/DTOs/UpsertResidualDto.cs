using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

public record UpsertResidualDto(Guid? Id, string Content, Coordinates Coordinates);
