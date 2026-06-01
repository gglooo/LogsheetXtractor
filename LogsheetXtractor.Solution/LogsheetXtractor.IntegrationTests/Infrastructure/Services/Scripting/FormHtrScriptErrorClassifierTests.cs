using FluentAssertions;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Infrastructure.Services.Scripting;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Scripting;

public class FormHtrScriptErrorClassifierTests
{
    private readonly FormHtrScriptErrorClassifier _classifier = new();

    [Fact]
    public void ClassifyProcessLogsheetFailure_ShouldReturnInvalidCredentials_ForGoogleServiceAccountJsonError()
    {
        const string error = """
            Traceback (most recent call last):
              File "/app/venv/lib/python3.12/site-packages/google/oauth2/service_account.py", line 270, in from_service_account_file
                info, signer = _service_account_info.from_filename(
              File "/app/venv/lib/python3.12/site-packages/google/auth/_service_account_info.py", line 79, in from_filename
                data = json.load(json_file)
            json.decoder.JSONDecodeError: Expecting value: line 1 column 1 (char 0)
            """;

        var result = _classifier.ClassifyProcessLogsheetFailure(error);

        result.Should().Be(ScriptFailureKind.OcrCredentialsInvalid);
    }

    [Fact]
    public void ClassifyProcessLogsheetFailure_ShouldReturnInvalidCredentials_ForGooglePermissionError()
    {
        const string error =
            "google.api_core.exceptions.PermissionDenied: 403 Permission denied";

        var result = _classifier.ClassifyProcessLogsheetFailure(error);

        result.Should().Be(ScriptFailureKind.OcrCredentialsInvalid);
    }

    [Fact]
    public void ClassifyProcessLogsheetFailure_ShouldReturnInvalidCredentials_ForMalformedGoogleServiceAccount()
    {
        const string error = """
            File "/app/venv/lib/python3.12/site-packages/google/oauth2/service_account.py", line 270, in from_service_account_file
              info, signer = _service_account_info.from_filename(
            File "/app/venv/lib/python3.12/site-packages/google/auth/_service_account_info.py", line 50, in from_dict
              raise exceptions.MalformedError(
            google.auth.exceptions.MalformedError: Service account info was not in the expected format, missing fields client_email, token_uri.
            """;

        var result = _classifier.ClassifyProcessLogsheetFailure(error);

        result.Should().Be(ScriptFailureKind.OcrCredentialsInvalid);
    }

    [Fact]
    public void ClassifyProcessLogsheetFailure_ShouldReturnUnknown_ForUnrelatedTraceback()
    {
        const string error = "Traceback: ValueError: failed to parse logsheet coordinates";

        var result = _classifier.ClassifyProcessLogsheetFailure(error);

        result.Should().Be(ScriptFailureKind.Unknown);
    }
}
