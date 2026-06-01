using FluentResults;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Logsheets;

public interface ILogsheetService
{
    Task<Result<LogsheetDetailDto>> AlignLogsheetAsync(Logsheet logsheet, CancellationToken ct);

    Task<Result<LogsheetDetailDto>> ProcessLogsheetAsync(
        Logsheet logsheet,
        ProcessLogsheetDataOptions? options,
        CancellationToken ct
    );
}
