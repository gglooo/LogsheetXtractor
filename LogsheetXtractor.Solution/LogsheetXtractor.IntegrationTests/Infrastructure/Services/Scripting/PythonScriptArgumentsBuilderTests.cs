using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Infrastructure.Services.Scripting;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Scripting;

public class PythonScriptArgumentsBuilderTests
{
    private readonly PythonScriptArgumentsBuilder _builder = new();

    [Fact]
    public void BuildSelectRoisArguments_ShouldIncludeCredentialFlags_WhenCredentialsProvided()
    {
        var args = _builder.BuildSelectRoisArguments("input.pdf", "out.json", "/creds.json");

        args.Should()
            .ContainInOrder(
                PythonCliArgs.PdfFile,
                "input.pdf",
                PythonCliArgs.OutputFile,
                "out.json",
                PythonCliArgs.Autodetect,
                PythonCliArgs.Headless
            );
        args.Should().Contain(PythonCliArgs.DetectResiduals);
        args.Should().Contain(PythonCliArgs.Credentials);
        args.Should().Contain("/creds.json");
    }

    [Fact]
    public void BuildProcessLogsheetArguments_ShouldComposeBaseCredentialsAndOptionalFlags()
    {
        var args = _builder.BuildProcessLogsheetArguments(
            "output.csv",
            "template.pdf",
            "logsheet.pdf",
            "config.json",
            new List<(ECredentialType, string)>
            {
                (ECredentialType.Google, "/google.json"),
                (ECredentialType.Azure, "/azure.json"),
            },
            new PreparedAlignmentInput(
                IsAligned: false,
                AlignmentConfigPath: "align.json",
                BacksideAlignmentConfigPath: null
            ),
            new PreparedBacksideInput("back.pdf", "back-config.json"),
            uglyCheckboxes: true
        );

        args.Should()
            .ContainInOrder(
                PythonCliArgs.OutputFile,
                "output.csv",
                PythonCliArgs.PdfTemplate,
                "template.pdf",
                PythonCliArgs.PdfLogsheet,
                "logsheet.pdf",
                PythonCliArgs.ConfigFile,
                "config.json",
                PythonCliArgs.GoogleCredential,
                "/google.json",
                PythonCliArgs.AzureCredential,
                "/azure.json"
            );
        args.Should().Contain(PythonCliArgs.AlignmentConfig);
        args.Should().Contain("align.json");
        args.Should().Contain(PythonCliArgs.Backside);
        args.Should().Contain(PythonCliArgs.BacksideTemplate);
        args.Should().Contain("back.pdf");
        args.Should().Contain(PythonCliArgs.BacksideConfig);
        args.Should().Contain("back-config.json");
        args.Should().Contain(PythonCliArgs.StoreCsv);
        args.Should().Contain(PythonCliArgs.UglyCheckboxes);
    }

    [Fact]
    public void BuildAutomaticAlignmentArguments_ShouldIncludeBacksideTemplate_WhenProvided()
    {
        var args = _builder.BuildAutomaticAlignmentArguments(
            "logsheet.pdf",
            "template.pdf",
            "backside.pdf"
        );

        args.Should()
            .ContainInOrder(
                PythonCliArgs.PdfLogsheet,
                "logsheet.pdf",
                PythonCliArgs.PdfTemplate,
                "template.pdf",
                PythonCliArgs.BacksideTemplate,
                "backside.pdf"
            );
    }

    [Fact]
    public void BuildExportLogsheetArguments_ShouldComposeBaseAndAdditionalArguments()
    {
        var args = _builder.BuildExportLogsheetArguments(
            "logsheet.pdf",
            "template.pdf",
            "export-config.json",
            "out.csv",
            new PreparedAlignmentInput(
                IsAligned: false,
                AlignmentConfigPath: "align.json",
                BacksideAlignmentConfigPath: "back-align.json"
            ),
            new PreparedBacksideInput("back.pdf", "back-config.json")
        );

        args.Should()
            .ContainInOrder(
                PythonCliArgs.PdfLogsheet,
                "logsheet.pdf",
                PythonCliArgs.PdfTemplate,
                "template.pdf",
                PythonCliArgs.ConfigFile,
                "export-config.json",
                PythonCliArgs.OutputFile,
                "out.csv",
                PythonCliArgs.AlignmentConfig,
                "align.json",
                PythonCliArgs.BacksideAlignmentConfig,
                "back-align.json"
            );
        args.Should().Contain(PythonCliArgs.Backside);
        args.Should().Contain(PythonCliArgs.BacksideTemplate);
        args.Should().Contain("back.pdf");
        args.Should().NotContain(PythonCliArgs.BacksideConfig);
        args.Should().NotContain("back-config.json");
    }

    [Fact]
    public void BuildExportLogsheetArguments_ShouldIncludeAlignedFlag_WhenInputIsAligned()
    {
        var args = _builder.BuildExportLogsheetArguments(
            "logsheet.pdf",
            "template.pdf",
            "export-config.json",
            "out.csv",
            new PreparedAlignmentInput(
                IsAligned: true,
                AlignmentConfigPath: null,
                BacksideAlignmentConfigPath: null
            ),
            backsideInput: null
        );

        args.Should().Contain(PythonCliArgs.Aligned);
        args.Should().NotContain(PythonCliArgs.AlignmentConfig);
        args.Should().NotContain(PythonCliArgs.BacksideAlignmentConfig);
        args.Should().NotContain(PythonCliArgs.Backside);
    }

    [Fact]
    public void BuildPdfDimensionsArguments_ShouldReturnPdfFileFlag()
    {
        var args = _builder.BuildPdfDimensionsArguments("input.pdf");

        args.Should().Equal(PythonCliArgs.PdfFile, "input.pdf");
    }
}
