using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public static class PythonCliArgs
{
    public const string PdfFile = "--pdf-file";
    public const string OutputFile = "--output-file";
    public const string Autodetect = "--autodetect";
    public const string Headless = "--headless";
    public const string DetectResiduals = "--detect-residuals";
    public const string Credentials = "--credentials";
    public const string PdfLogsheet = "--pdf-logsheet";
    public const string PdfTemplate = "--pdf-template";
    public const string BacksideTemplate = "--backside-template";
    public const string ConfigFile = "--config-file";
    public const string StoreCsv = "--store-csv";
    public const string UglyCheckboxes = "--ugly-checkboxes";
    public const string Aligned = "--aligned";
    public const string AlignmentConfig = "--alignment-config";
    public const string BacksideAlignmentConfig = "--backside-alignment-config";
    public const string Backside = "--backside";
    public const string BacksideConfig = "--backside-config";
    public const string GoogleCredential = "--google";
    public const string AzureCredential = "--azure";
    public const string AmazonCredential = "--amazon";

    public static string CredentialTypeFlag(ECredentialType credentialType)
    {
        return credentialType switch
        {
            ECredentialType.Google => GoogleCredential,
            ECredentialType.Azure => AzureCredential,
            ECredentialType.Amazon => AmazonCredential,
            _ => throw new ArgumentOutOfRangeException(nameof(credentialType), credentialType, null),
        };
    }
}
