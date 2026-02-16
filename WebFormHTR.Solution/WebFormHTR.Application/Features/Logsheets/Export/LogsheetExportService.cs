using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Logsheets.Export;

public class LogsheetExportService(IAppDbContext dbContext, IHtrScriptEngine scriptEngine, IMapper mapper)
    : ILogsheetExportService
{
    public async Task<Result<GetFileDto>> ExportLogsheetDataAsync(Guid logsheetId, CancellationToken ct)
    {
        var logsheet = await dbContext.Logsheets
            .Include(l => l.ExtractedValues)
            .Include(l => l.Template)
            .ThenInclude(t => t.Rois)
            .Include(l => l.Template)
            .ThenInclude(t => t.BacksideTemplate)
            .ThenInclude(t => t.Rois)
            .Include(l => l.Template)
            .ThenInclude(t => t.BacksideTemplate)
            .ThenInclude(t => t.File)
            .FirstOrDefaultAsync(ls => ls.Id == logsheetId, ct);

        if (logsheet is null)
        {
            return Result.Fail(new NotFoundError("Logsheet not found"));
        }

        List<ExportLogsheetDataDto> exportData = [];
        exportData.AddRange(logsheet.ExtractedValues.Select(extractedValue => new ExportLogsheetDataDto
        {
            VariableName = extractedValue.Roi.VariableName, Value = extractedValue.Value,
            Coordinates = mapper.Map<ExportCoordinateDto>(extractedValue.Roi.Coordinates),
            Page = extractedValue.IsBackside ? 1 : 0
        }));


        try
        {
            var fileResult =
                await scriptEngine.ExportLogsheetDataAsync(logsheet, exportData, logsheet.File,
                    logsheet.Template.File, ct);
            return fileResult;
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error($"Failed to export logsheet data: {ex.Message}"));
        }
    }
}