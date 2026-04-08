using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public class PythonScriptArgumentsBuilder : IPythonScriptArgumentsBuilder
{
    public IReadOnlyList<string> BuildSelectRoisArguments(
        string inputFilePath,
        string outputFilePath,
        string? usedCredentialsPath
    )
    {
        var args = new List<string>
        {
            PythonCliArgs.PdfFile,
            inputFilePath,
            PythonCliArgs.OutputFile,
            outputFilePath,
            PythonCliArgs.Autodetect,
            PythonCliArgs.Headless,
        };

        if (!string.IsNullOrWhiteSpace(usedCredentialsPath))
        {
            args.Add(PythonCliArgs.DetectResiduals);
            args.Add(PythonCliArgs.Credentials);
            args.Add(usedCredentialsPath);
        }

        return args;
    }

    public IReadOnlyList<string> BuildAutomaticAlignmentArguments(
        string logsheetPath,
        string templatePath,
        string? backsideTemplatePath
    )
    {
        var args = new List<string>
        {
            PythonCliArgs.PdfLogsheet,
            logsheetPath,
            PythonCliArgs.PdfTemplate,
            templatePath,
        };

        if (!string.IsNullOrWhiteSpace(backsideTemplatePath))
        {
            args.Add(PythonCliArgs.BacksideTemplate);
            args.Add(backsideTemplatePath);
        }

        return args;
    }

    public IReadOnlyList<string> BuildProcessLogsheetArguments(
        string outputFilePath,
        string templatePath,
        string logsheetPath,
        string configPath,
        IEnumerable<(ECredentialType CredentialType, string CredentialPath)> credentials,
        PreparedAlignmentInput alignmentInput,
        PreparedBacksideInput? backsideInput,
        bool uglyCheckboxes
    )
    {
        var args = new List<string>
        {
            PythonCliArgs.OutputFile,
            outputFilePath,
            PythonCliArgs.PdfTemplate,
            templatePath,
            PythonCliArgs.PdfLogsheet,
            logsheetPath,
            PythonCliArgs.ConfigFile,
            configPath,
        };

        foreach (var (credentialType, credentialPath) in credentials)
        {
            args.Add(PythonCliArgs.CredentialTypeFlag(credentialType));
            args.Add(credentialPath);
        }

        AddAlignmentArguments(args, alignmentInput);
        AddBacksideArguments(args, backsideInput);
        args.Add(PythonCliArgs.StoreCsv);

        if (uglyCheckboxes)
        {
            args.Add(PythonCliArgs.UglyCheckboxes);
        }

        return args;
    }

    public IReadOnlyList<string> BuildPdfDimensionsArguments(string filePath)
    {
        return [PythonCliArgs.PdfFile, filePath];
    }

    public IReadOnlyList<string> BuildExportLogsheetArguments(
        string logsheetPath,
        string templatePath,
        string configPath,
        string outputFilePath,
        PreparedAlignmentInput alignmentInput,
        PreparedBacksideInput? backsideInput
    )
    {
        var args = new List<string>
        {
            PythonCliArgs.PdfLogsheet,
            logsheetPath,
            PythonCliArgs.PdfTemplate,
            templatePath,
            PythonCliArgs.ConfigFile,
            configPath,
            PythonCliArgs.OutputFile,
            outputFilePath,
        };

        AddAlignmentArguments(args, alignmentInput);
        AddBacksideArguments(args, backsideInput);

        return args;
    }

    private static void AddAlignmentArguments(
        ICollection<string> args,
        PreparedAlignmentInput alignmentInput
    )
    {
        if (alignmentInput.IsAligned)
        {
            args.Add(PythonCliArgs.Aligned);
            return;
        }

        if (!string.IsNullOrWhiteSpace(alignmentInput.AlignmentConfigPath))
        {
            args.Add(PythonCliArgs.AlignmentConfig);
            args.Add(alignmentInput.AlignmentConfigPath);
        }

        if (!string.IsNullOrWhiteSpace(alignmentInput.BacksideAlignmentConfigPath))
        {
            args.Add(PythonCliArgs.BacksideAlignmentConfig);
            args.Add(alignmentInput.BacksideAlignmentConfigPath);
        }
    }

    private static void AddBacksideArguments(
        ICollection<string> args,
        PreparedBacksideInput? backsideInput
    )
    {
        if (backsideInput is null)
        {
            return;
        }

        args.Add(PythonCliArgs.Backside);
        args.Add(PythonCliArgs.BacksideTemplate);
        args.Add(backsideInput.BacksideTemplatePath);
        args.Add(PythonCliArgs.BacksideConfig);
        args.Add(backsideInput.BacksideConfigPath);
    }
}
