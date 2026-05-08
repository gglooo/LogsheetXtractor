using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Residuals.DTOs;

public record CreateResidualDto(string Content, Coordinates Coordinates);
