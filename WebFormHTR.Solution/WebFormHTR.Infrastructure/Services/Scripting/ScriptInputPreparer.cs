using System.Text;
using System.Text.Json;
using MapsterMapper;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public class ScriptInputPreparer(
    IFileStorageService fileStorageService,
    IMapper mapper) : IScriptInputPreparer
{
    public async Task<string> CreateTemplateConfigAsync(Template template, CancellationToken ct)
    {
        return await GenerateConfigFileAsync(template, ct);
    }

    public async Task<string> CreateAlignmentArgumentAsync(Logsheet logsheet, CancellationToken ct)
    {
        var frontsidePoints = logsheet.AlignmentData.Frontside;
        var backsidePoints = logsheet.AlignmentData.Backside;

        var hasFrontsidePoints = frontsidePoints != null && frontsidePoints.Count > 0;
        var hasBacksidePoints = backsidePoints != null && backsidePoints.Count > 0;
        var anyPoints = hasFrontsidePoints || hasBacksidePoints;

        if (!anyPoints)
        {
            return BuildAlignedArgument();
        }

        // If one side is aligned strategies are mutually exclusive (manual points vs --aligned)
        // so we must provide points for the other side too (Identity points)
        if (!hasFrontsidePoints)
        {
            frontsidePoints = GetIdentityPoints(logsheet.Template);
        }
        
        if (logsheet.Template.BacksideTemplate is not null && !hasBacksidePoints)
        {
            backsidePoints = GetIdentityPoints(logsheet.Template.BacksideTemplate);
        }

        var frontsideAlignmentConfigPath = await GetTemplateConfigPath(logsheet.Template, frontsidePoints);
        var backsideAlignmentConfigPath = logsheet.Template.BacksideTemplate is not null
            ? await GetTemplateConfigPath(logsheet.Template.BacksideTemplate, backsidePoints)
            : null;

        var alignmentArgs = new List<string>();
        if (frontsideAlignmentConfigPath is not null)
        {
            alignmentArgs.Add(BuildAlignmentArgument(frontsideAlignmentConfigPath));
        }

        if (backsideAlignmentConfigPath is not null)
        {
            alignmentArgs.Add(BuildBacksideAlignmentArgument(backsideAlignmentConfigPath));
        }

        return string.Join(" ", alignmentArgs);
    }

    public async Task<string> CreateBacksideArgumentAsync(Logsheet logsheet, CancellationToken ct)
    {
        var backTemplate = logsheet.Template.BacksideTemplate;
        if (backTemplate is null)
        {
            return string.Empty;
        }

        var configPath = await GenerateConfigFileAsync(backTemplate, ct);
        var pdfPath = fileStorageService.GetResolvedPath(backTemplate.File.StoragePath);

        return $"--backside --backside_template {pdfPath} --backside_config {configPath}";
    }

    private async Task<string> GenerateConfigFileAsync(Template template, CancellationToken ct)
    {
        var templateConfig = mapper.Map<PythonTemplateConfig>(template);
        var configJson = JsonSerializer.Serialize(templateConfig);

        return await fileStorageService.SaveTemporaryFileAsync(
            Encoding.UTF8.GetBytes(configJson),
            $"{Guid.NewGuid()}.json",
            ct);
    }

    private static string BuildAlignmentArgument(string configPath)
    {
        return $"--alignment_config {configPath}";
    }

    private static string BuildBacksideAlignmentArgument(string configPath)
    {
        return $"--backside_alignment_config {configPath}";
    }

    private static string BuildAlignedArgument()
    {
        return "--aligned";
    }

    private async Task<string?> GetTemplateConfigPath(Template template, List<PointCoordinate>? alignmentPoints)
    {
        if (!DoPointsNeedAlignment(alignmentPoints))
        {
            return null;
        }

        var w = template.Width ?? 0;
        var h = template.Height ?? 0;
        if (w == 0 || h == 0)
        {
            throw new InvalidOperationException("Template dimensions are required for alignment configuration.");
        }

        var templateCorners = new List<PointCoordinate>
        {
            new(0, 0),
            new(w, 0),
            new(w, h),
            new(0, h)
        };

        var alignmentConfig = new PythonAlignmentConfig(templateCorners, alignmentPoints);
        var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alignmentConfig));

        return await fileStorageService.SaveTemporaryFileAsync(
            jsonBytes,
            $"{Guid.NewGuid()}_alignment_config.json",
            CancellationToken.None);
    }

    private bool DoPointsNeedAlignment(List<PointCoordinate>? points)
    {
        return points != null && points.Count > 0;
    }

    private static List<PointCoordinate> GetIdentityPoints(Template template)
    {
        var w = template.Width ?? 0;
        var h = template.Height ?? 0;
        return new List<PointCoordinate>
        {
            new(0, 0),
            new(w, 0),
            new(w, h),
            new(0, h)
        };
    }
}