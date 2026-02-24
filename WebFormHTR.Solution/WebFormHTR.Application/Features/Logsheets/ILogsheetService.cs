using FluentResults;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Logsheets;

public interface ILogsheetService
{
    Task<Result<LogsheetDetailDto>> ProcessLogsheetAsync(Logsheet logsheet, CancellationToken ct);

    Task<Result<IEnumerable<LogsheetDetailDto>>> ProcessLogsheetsAsync(IEnumerable<Logsheet> logsheets,
        CancellationToken ct);
}