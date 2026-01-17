using FluentResults;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Logsheets;

public interface ILogsheetService
{
    Error? ValidateLogsheet(Logsheet? logsheet);
    Task<LogsheetDetailDto> ProcessLogsheetAsync(Logsheet logsheet, CancellationToken ct);
    Task<IEnumerable<LogsheetDetailDto>> ProcessLogsheetsAsync(IEnumerable<Logsheet> logsheets, CancellationToken ct);
}