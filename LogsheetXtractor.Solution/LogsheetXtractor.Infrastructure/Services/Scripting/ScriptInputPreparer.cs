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

    public async Task<PreparedAlignmentInput> PrepareAlignmentInputAsync(
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
            return new PreparedAlignmentInput(
                IsAligned: true,
                AlignmentConfigPath: null,
                BacksideAlignmentConfigPath: null
            );
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

        return new PreparedAlignmentInput(
            IsAligned: false,
            AlignmentConfigPath: frontsideAlignmentConfigPath,
            BacksideAlignmentConfigPath: backsideAlignmentConfigPath
        );
    }

    public async Task<PreparedBacksideInput?> PrepareBacksideInputAsync(
        Logsheet logsheet,
        bool hasBacksidePage,
        CancellationToken ct
    )
    {
        if (!hasBacksidePage)
        {
            return null;
        }

        var backTemplate = logsheet.Template.BacksideTemplate;
        if (backTemplate is null)
        {
            return null;
        }

        var configPath = await GenerateConfigFileAsync(backTemplate, ct);
        var pdfPath = fileStorageService.GetResolvedPath(backTemplate.File.StoragePath);

        return new PreparedBacksideInput(pdfPath, configPath);
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
