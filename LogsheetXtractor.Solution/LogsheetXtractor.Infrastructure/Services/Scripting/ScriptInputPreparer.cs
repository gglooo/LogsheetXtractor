using System.Text;
using System.Text.Json;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;
using LogsheetXtractor.Infrastructure.Services.Storage;
using MapsterMapper;

namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public class ScriptInputPreparer(IFileStorageService fileStorageService, IMapper mapper)
    : IScriptInputPreparer
{
    public async Task<string> CreateTemplateConfigAsync(Template template, CancellationToken ct)
    {
        return await GenerateConfigFileAsync(template, ct);
    }

    public async Task<IEnumerable<string>> CreateAlignmentArgumentAsync(
        Logsheet logsheet,
        bool hasBacksidePage,
        CancellationToken ct
    )
    {
        var frontsidePoints = logsheet.AlignmentData.Frontside;
        var backsidePoints = logsheet.AlignmentData.Backside;

        var hasFrontsidePoints = frontsidePoints != null && frontsidePoints.Count > 0;
        var hasBacksidePoints = backsidePoints != null && backsidePoints.Count > 0;
        var anyPoints = hasFrontsidePoints || hasBacksidePoints;

        if (!anyPoints)
        {
            return new[] { "--aligned" };
        }

        if (!hasFrontsidePoints)
        {
            frontsidePoints = GetIdentityPoints(logsheet.Template);
        }

        if (hasBacksidePage && logsheet.Template.BacksideTemplate is not null && !hasBacksidePoints)
        {
            backsidePoints = GetIdentityPoints(logsheet.Template.BacksideTemplate);
        }

        var frontsideAlignmentConfigPath = await GetTemplateConfigPath(
            logsheet.Template,
            frontsidePoints
        );
        var backsideAlignmentConfigPath =
            hasBacksidePage && logsheet.Template.BacksideTemplate is not null
                ? await GetTemplateConfigPath(logsheet.Template.BacksideTemplate, backsidePoints)
                : null;

        var alignmentArgs = new List<string>();
        if (frontsideAlignmentConfigPath is not null)
        {
            alignmentArgs.Add("--alignment_config");
            alignmentArgs.Add(frontsideAlignmentConfigPath);
        }

        if (backsideAlignmentConfigPath is not null)
        {
            alignmentArgs.Add("--backside_alignment_config");
            alignmentArgs.Add(backsideAlignmentConfigPath);
        }

        return alignmentArgs;
    }

    public async Task<IEnumerable<string>> CreateBacksideArgumentAsync(
        Logsheet logsheet,
        bool hasBacksidePage,
        CancellationToken ct
    )
    {
        if (!hasBacksidePage)
        {
            return Array.Empty<string>();
        }

        var backTemplate = logsheet.Template.BacksideTemplate;
        if (backTemplate is null)
        {
            return Array.Empty<string>();
        }

        var configPath = await GenerateConfigFileAsync(backTemplate, ct);
        var pdfPath = fileStorageService.GetResolvedPath(backTemplate.File.StoragePath);

        return new[]
        {
            "--backside",
            "--backside_template",
            pdfPath,
            "--backside_config",
            configPath,
        };
    }

    private async Task<string> GenerateConfigFileAsync(Template template, CancellationToken ct)
    {
        var templateConfig = mapper.Map<PythonTemplateConfig>(template);
        var configJson = JsonSerializer.Serialize(templateConfig);

        return await fileStorageService.SaveTemporaryFileAsync(
            Encoding.UTF8.GetBytes(configJson),
            $"{Guid.NewGuid()}.json",
            ct
        );
    }

    private async Task<string?> GetTemplateConfigPath(
        Template template,
        List<PointCoordinate>? alignmentPoints
    )
    {
        if (!DoPointsNeedAlignment(alignmentPoints))
        {
            return null;
        }

        var w = template.Width ?? 0;
        var h = template.Height ?? 0;
        if (w == 0 || h == 0)
        {
            throw new InvalidOperationException(
                "Template dimensions are required for alignment configuration."
            );
        }

        var templateCorners = new List<PointCoordinate>
        {
            new(0, 0),
            new(w, 0),
            new(w, h),
            new(0, h),
        };

        var alignmentConfig = new PythonAlignmentConfig(templateCorners, alignmentPoints);
        var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alignmentConfig));

        return await fileStorageService.SaveTemporaryFileAsync(
            jsonBytes,
            $"{Guid.NewGuid()}_alignment_config.json",
            CancellationToken.None
        );
    }

    private bool DoPointsNeedAlignment(List<PointCoordinate>? points)
    {
        return points != null && points.Count > 0;
    }

    private static List<PointCoordinate> GetIdentityPoints(Template template)
    {
        var w = template.Width ?? 0;
        var h = template.Height ?? 0;
        return new List<PointCoordinate> { new(0, 0), new(w, 0), new(w, h), new(0, h) };
    }
}
