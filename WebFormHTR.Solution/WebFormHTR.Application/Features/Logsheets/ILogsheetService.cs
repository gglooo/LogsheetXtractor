using FluentResults;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Logsheets;

public interface ILogsheetService
{
    Task<Result<LogsheetDetailDto>> ProcessLogsheetAsync(Logsheet logsheet, ProcessLogsheetDataOptions? options,
        CancellationToken ct);

    Task<Result<IEnumerable<LogsheetDetailDto>>> ProcessLogsheetsAsync(IEnumerable<Logsheet> logsheets,
        ProcessLogsheetDataOptions? options,
        CancellationToken ct);
}