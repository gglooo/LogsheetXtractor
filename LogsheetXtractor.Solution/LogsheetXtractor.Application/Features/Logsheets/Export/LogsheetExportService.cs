using System.IO.Compression;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Logsheets.Export;

public class LogsheetExportService(
    IAppDbContext dbContext,
    IHtrScriptEngine scriptEngine,
    IMapper mapper
) : ILogsheetExportService
{
    public async Task<Result<GetFileDto>> ExportLogsheetDataAsync(
        Guid logsheetId,
        CancellationToken ct
    )
    {
        var logsheet = await CreateExportLogsheetQuery()
            .FirstOrDefaultAsync(ls => ls.Id == logsheetId, ct);

        if (logsheet is null || logsheet.Status != ELogSheetStatus.Completed)
        {
            return Result.Fail<GetFileDto>(
                new Error($"Logsheet {logsheetId} is not available for export")
            );
        }

        return await ExportLoadedLogsheetDataAsync(logsheet, ct);
    }

    public async Task<Result<GetFileDto>> ExportBatchLogsheetDataAsync(
        IEnumerable<Guid> logsheetIds,
        CancellationToken ct
    )
    {
        var requestedLogsheetIds = logsheetIds.Distinct().ToArray();
        var logsheets = await CreateExportLogsheetQuery()
            .Where(ls =>
                requestedLogsheetIds.Contains(ls.Id) && ls.Status == ELogSheetStatus.Completed
            )
            .ToListAsync(ct);

        if (logsheets.Count == 0)
        {
            return Result.Fail<GetFileDto>(
                new InvalidStateError("No logsheets available for export")
            );
        }

        var tempZipPath = Path.GetTempFileName();

        try
        {
            var exportedCount = 0;
            await using (var fileStream = new FileStream(tempZipPath, FileMode.Create))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    foreach (var logsheet in logsheets)
                    {
                        var result = await ExportLoadedLogsheetDataAsync(logsheet, ct);
                        if (result.IsFailed)
                        {
                            continue;
                        }

                        var fileDto = result.Value;
                        if (fileDto.Stream is null || string.IsNullOrWhiteSpace(fileDto.FileName))
                        {
                            continue;
                        }

                        await using var sourceStream = fileDto.Stream;
                        var entry = archive.CreateEntry(fileDto.FileName);
                        await using var entryStream = entry.Open();
                        await sourceStream.CopyToAsync(entryStream, ct);
                        exportedCount++;
                    }
                }
            }

            if (exportedCount == 0)
            {
                return Result.Fail<GetFileDto>(
                    new InvalidStateError("No logsheets available for export")
                );
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
                Stream = memoryStream,
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

    private IQueryable<Logsheet> CreateExportLogsheetQuery()
    {
        return dbContext
            .Logsheets.AsNoTracking()
            .Include(l => l.File)
            .Include(l => l.ExtractedValues)
            .ThenInclude(ev => ev.Roi)
            .Include(l => l.Template)
            .ThenInclude(t => t.File)
            .Include(l => l.Template)
            .ThenInclude(t => t.BacksideTemplate!)
            .ThenInclude(t => t.Rois)
            .Include(l => l.Template)
            .ThenInclude(t => t.BacksideTemplate!)
            .ThenInclude(t => t.File);
    }

    private async Task<Result<GetFileDto>> ExportLoadedLogsheetDataAsync(
        Logsheet logsheet,
        CancellationToken ct
    )
    {
        if (logsheet.Template is null || logsheet.Template.File is null || logsheet.File is null)
        {
            return Result.Fail<GetFileDto>(
                new ValidationError($"Logsheet {logsheet.Id} is missing export dependencies")
            );
        }

        var template = logsheet.Template;
        var backsideRoiIds =
            template.BacksideTemplate?.Rois.Select(roi => roi.Id).ToHashSet() ?? [];

        List<ExportLogsheetDataDto> exportData = [];
        exportData.AddRange(
            logsheet
                .ExtractedValues.Where(extractedValue => extractedValue.Roi is not null)
                .Select(extractedValue => new ExportLogsheetDataDto
                {
                    VariableName = extractedValue.Roi.VariableName,
                    Value = extractedValue.CorrectedValue ?? extractedValue.Value,
                    Coordinates = mapper.Map<ExportCoordinateDto>(extractedValue.Roi.Coordinates),
                    Page = backsideRoiIds.Contains(extractedValue.RoiId) ? 1 : 0,
                })
        );

        try
        {
            return await scriptEngine.ExportLogsheetDataAsync(
                logsheet,
                exportData,
                logsheet.File,
                template.File,
                ct
            );
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error($"Failed to export logsheet data: {ex.Message}"));
        }
    }
}
