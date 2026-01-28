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
        var frontsidePoints = logsheet.AlignmentDataModelConfig.Frontside;
        var backsidePoints = logsheet.AlignmentDataModelConfig.Backside;

        var frontsideAlignmentConfigPath = await GetTemplateConfigPath(logsheet.Template, frontsidePoints);
        var backsideAlignmentConfigPath = logsheet.Template.BacksideTemplate is not null
            ? await GetTemplateConfigPath(logsheet.Template.BacksideTemplate, backsidePoints)
            : null;

        if (frontsideAlignmentConfigPath is null && backsideAlignmentConfigPath is null)
        {
            return BuildAlignedArgument();
        }

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
            new() { X = 0, Y = 0 },
            new() { X = w, Y = 0 },
            new() { X = w, Y = h },
            new() { X = 0, Y = h }
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
        return points is { Count: >= 0 };
    }
}