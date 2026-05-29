using FluentResults;
using LogsheetXtractor.Application.DTOs;

namespace LogsheetXtractor.Application.Features.Logsheets.Export;

public interface ILogsheetExportService
{
    Task<Result<GetFileDto>> ExportLogsheetDataAsync(Guid logsheetId, CancellationToken ct);
    Task<Result<GetFileDto>> ExportBatchLogsheetDataAsync(
        IEnumerable<Guid> logsheetIds,
        CancellationToken ct
    );
}
