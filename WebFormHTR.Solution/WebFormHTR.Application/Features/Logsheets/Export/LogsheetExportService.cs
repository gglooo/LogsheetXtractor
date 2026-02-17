using System.IO.Compression;
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

    public async Task<Result<GetFileDto>> ExportBatchLogsheetDataAsync(IEnumerable<Guid> logsheetIds, CancellationToken ct)
    {
        var tempZipPath = Path.GetTempFileName();
        
        try
        {
            await using (var fileStream = new FileStream(tempZipPath, FileMode.Create))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    foreach (var logsheetId in logsheetIds)
                    {
                        var result = await ExportLogsheetDataAsync(logsheetId, ct);
                        if (result.IsFailed)
                        {
                            return Result.Fail<GetFileDto>($"Failed to export logsheet {logsheetId}: {result.Errors.First().Message}");
                        }
                        
                        var fileDto = result.Value;
                        var entry = archive.CreateEntry(fileDto.FileName);
                        await using var entryStream = entry.Open();
                        await fileDto.Stream.CopyToAsync(entryStream, ct);
                    }
                }
            }

            var memoryStream = new MemoryStream();
            await using (var fileStream = new FileStream(tempZipPath, FileMode.Open))
            {
                await fileStream.CopyToAsync(memoryStream, ct);
            }
            memoryStream.Position = 0;
            
            System.IO.File.Delete(tempZipPath);

            return new GetFileDto
            {
                FileName = $"batch_export_{Guid.NewGuid()}.zip",
                ContentType = "application/zip",
                Stream = memoryStream
            };
        }
        catch (Exception ex)
        {
            if (System.IO.File.Exists(tempZipPath))
            {
                System.IO.File.Delete(tempZipPath);
            }
            return Result.Fail<GetFileDto>($"Failed to create batch export: {ex.Message}");
        }
    }
}