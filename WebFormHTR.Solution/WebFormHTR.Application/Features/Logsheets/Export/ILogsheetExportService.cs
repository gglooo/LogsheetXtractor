using FluentResults;
using WebFormHTR.Application.DTOs;

namespace WebFormHTR.Application.Features.Logsheets.Export;

public interface ILogsheetExportService
{
    Task<Result<GetFileDto>> ExportLogsheetDataAsync(Guid logsheetId, CancellationToken ct);
}