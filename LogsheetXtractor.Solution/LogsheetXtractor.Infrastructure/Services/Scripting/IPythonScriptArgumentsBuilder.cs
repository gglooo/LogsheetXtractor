using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public interface IPythonScriptArgumentsBuilder
{
    IReadOnlyList<string> BuildSelectRoisArguments(
        string inputFilePath,
        string outputFilePath,
        string? usedCredentialsPath
    );

    IReadOnlyList<string> BuildAutomaticAlignmentArguments(
        string logsheetPath,
        string templatePath,
        string? backsideTemplatePath
    );

    IReadOnlyList<string> BuildProcessLogsheetArguments(
        string outputFilePath,
        string templatePath,
        string logsheetPath,
        string configPath,
        IEnumerable<(ECredentialType CredentialType, string CredentialPath)> credentials,
        PreparedAlignmentInput alignmentInput,
        PreparedBacksideInput? backsideInput,
        bool uglyCheckboxes
    );

    IReadOnlyList<string> BuildPdfDimensionsArguments(string filePath);

    IReadOnlyList<string> BuildExportLogsheetArguments(
        string logsheetPath,
        string templatePath,
        string configPath,
        string outputFilePath,
        PreparedAlignmentInput alignmentInput,
        PreparedBacksideInput? backsideInput
    );
}
