using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Extensions;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record IdentifyTemplatesFromFileQuery(Guid Id);

public static class IdentifyTemplatesFromFileHandler
{
    public static async Task<Result<Dictionary<int, TemplateListDto>>> Handle(IdentifyTemplatesFromFileQuery request,
        IAppDbContext dbContext, IFileService fileService, IPdfQrCodeScanner qrCodeScanner, IMapper mapper,
        CancellationToken ct)
    {
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == request.Id, ct);
        if (file is null)
        {
            return Result.Fail(new NotFoundError("File not found"));
        }

        var pdfStream = (await fileService.GetFileAsync(file.Id))?.Stream;
        if (pdfStream is null)
        {
            return Result.Fail(new NotFoundError("File stream not found"));
        }

        var qrCodeData = qrCodeScanner.DetectTemplates(pdfStream.ToByteArray());
        var scannedNames = qrCodeData
            .Aggregate(new HashSet<string>(), (set, data) =>
            {
                if (!string.IsNullOrWhiteSpace(data.Value))
                {
                    set.Add(data.Value);
                }

                return set;
            });

        var templatesFromDb = await dbContext.Templates
            .AsNoTracking()
            .Where(t => scannedNames.Contains(t.Name.ToString()))
            .ToListAsync(ct);

        var templateLookup = templatesFromDb.ToDictionary(t => t.Name.ToString(), t => t);

        var templates = new Dictionary<int, TemplateListDto>();
        foreach (var (pageIndex, templateName) in qrCodeData)
        {
            if (templateLookup.TryGetValue(templateName, out var template))
            {
                templates[pageIndex] = mapper.Map<TemplateListDto>(template);
            }
        }

        return templates;
    }
}