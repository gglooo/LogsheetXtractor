using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

public record SetResidualDto(Guid? Id, string Content, Coordinates Coordinates);
