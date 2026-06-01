using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.Export;

namespace LogsheetXtractor.Application.Features.Logsheets;

public sealed record ExportLogsheetDataCommand(Guid LogsheetId);

public static class ExportLogsheetDataHandler
{
    public static async Task<Result<GetFileDto>> Handle(
        ExportLogsheetDataCommand command,
        ILogsheetExportService logsheetExportService,
        CancellationToken ct
    )
    {
        try
        {
            var fileDto = await logsheetExportService.ExportLogsheetDataAsync(
                command.LogsheetId,
                ct
            );
            return fileDto;
        }
        catch (Exception ex)
        {
            return Result.Fail<GetFileDto>($"Failed to export logsheet data: {ex.Message}");
        }
    }
}
