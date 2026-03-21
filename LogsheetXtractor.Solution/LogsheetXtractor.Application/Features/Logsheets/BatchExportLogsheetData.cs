using System.IO.Compression;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.Export;

namespace LogsheetXtractor.Application.Features.Logsheets;

public sealed record BatchExportLogsheetDataCommand(Guid[] LogsheetIds);

public static class BatchExportLogsheetDataHandler
{
    public static async Task<Result<GetFileDto>> Handle(
        BatchExportLogsheetDataCommand command,
        ILogsheetExportService logsheetExportService,
        CancellationToken ct
    )
    {
        try
        {
            var fileDto = await logsheetExportService.ExportBatchLogsheetDataAsync(
                command.LogsheetIds,
                ct
            );
            return fileDto;
        }
        catch (Exception ex)
        {
            return Result.Fail<GetFileDto>($"Failed to export batch logsheet data: {ex.Message}");
        }
    }
}
